using System;
using System.Collections.Generic;
using System.Threading;
using StackExchange.Redis;
using Npgsql;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace RedisToPostgresWorker
{
    public class VoteResultsBehavior : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("WebSocket recibido: " + e.Data);
            Send("Respuesta del servidor: " + e.Data);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Configuración del servidor WebSocket
            var wsServer = new WebSocketServer("ws://0.0.0.0:8080"); // Configura el puerto 8080 para WebSockets
            wsServer.AddWebSocketService<VoteResultsBehavior>("/results"); // Punto de conexión para WebSockets
            wsServer.Start();

            // Conexión a Redis
            var redis = ConnectionMultiplexer.Connect("redis:6379");
            var db = redis.GetDatabase();

            // Conexión a PostgreSQL con reintentos
            var connString = "Host=db;Username=postgres;Password=mysecretpassword;Database=voting_db";
            var retries = 5;
            var delay = 2000;

            using var pgConn = TryConnect(connString, retries, delay);

            // Bucle para mantener el worker activo y emitir datos a través de WebSockets
            while (true)
            {
                try
                {
                    var dogVotes = db.StringGet("dog_votes");
                    var catVotes = db.StringGet("cat_votes");

                    using var cmd = new NpgsqlCommand("INSERT INTO vote_results (option, votes) VALUES (@option, @votes) ON CONFLICT (option) DO UPDATE SET votes = EXCLUDED.votes", pgConn);

                    cmd.Parameters.AddWithValue("option", "dogs");
                    cmd.Parameters.AddWithValue("votes", (int)dogVotes);
                    cmd.ExecuteNonQuery();

                    cmd.Parameters["option"].Value = "cats";
                    cmd.Parameters["votes"].Value = (int)catVotes;
                    cmd.ExecuteNonQuery();

                    Console.WriteLine("Datos actualizados en PostgreSQL");

                    // Enviar datos a través de WebSockets
                    var message = $"Perros: {dogVotes}, Gatos: {catVotes}";
                    wsServer.WebSocketServices["/results"].Sessions.Broadcast(message);

                    Thread.Sleep(10000); // Pausa antes del próximo ciclo
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en el worker: {ex.Message}");
                    break; // Salir del bucle si hay un error grave
                }
            }

            wsServer.Stop(); // Detener el servidor WebSocket al finalizar
        }

        static NpgsqlConnection TryConnect(string connString, int retries, int delay)
        {
            while (retries > 0)
            {
                try
                {
                    var pgConn = new NpgsqlConnection(connString);
                    pgConn.Open();
                    return pgConn; // Retorna si la conexión es exitosa
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error conectando a PostgreSQL: {ex.Message}");
                    Thread.Sleep(delay); // Esperar antes de reintentar
                    retries--; // Reduce el número de intentos restantes
                }
            }

            throw new Exception("No se pudo conectar a PostgreSQL después de múltiples intentos."); // Error si no se puede conectar
        }
    }
}
