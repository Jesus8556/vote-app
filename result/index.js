const express = require('express');
const path = require('path');
const { Client } = require('pg');
const WebSocket = require('ws');

// Crear una nueva instancia de Express
const app = express();

// Configuración de PostgreSQL
const dbClient = new Client({
  host: 'db',
  user: 'postgres',
  password: 'mysecretpassword',
  database: 'voting_db'
});

// Conectar a PostgreSQL
dbClient.connect();

// Configuración de WebSocket para conectarse al worker
const wsClient = new WebSocket('ws://worker:8080/results');

// Variable para almacenar los resultados
let voteResults = {
  dogs: 0,
  cats: 0
};

// Escuchar mensajes del worker
wsClient.on('message', (message) => {
  // Convertir a cadena antes de intentar dividir
  const textMessage = message.toString();
  console.log('Mensaje de WebSocket:', textMessage);

  // Asegurarse de que el mensaje sea un formato esperado antes de dividir
  if (textMessage.includes(', ')) {
    const [dogs, cats] = textMessage.split(', ');

    // Actualizar los resultados con el mensaje recibido
    voteResults.dogs = parseInt(dogs.split(': ')[1]);
    voteResults.cats = parseInt(cats.split(': ')[1]);
  } else {
    console.error('Formato de mensaje inesperado:', textMessage);
  }
});


// Servir la página HTML principal
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'index.html'));
});

// Endpoint para obtener resultados
app.get('/results', (req, res) => {
  res.json(voteResults);
});

// Iniciar el servidor Express
app.listen(3000, () => {
  console.log('Servidor Express ejecutándose en el puerto 3000');
});
