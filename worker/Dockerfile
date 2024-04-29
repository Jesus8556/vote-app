# Usar la imagen base de .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Establecer el directorio de trabajo dentro del contenedor
WORKDIR /app

# Copiar archivos del proyecto y restaurar dependencias
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Crear la imagen final para el runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

COPY --from=build-env /app/out .
EXPOSE 8080

# Definir el comando para ejecutar el servicio
CMD ["dotnet", "worker.dll"]
