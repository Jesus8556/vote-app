# Usar Node.js como base
FROM node:18-slim

# Establecer directorio de trabajo
WORKDIR /app

# Copiar archivos del proyecto
COPY . .

# Instalar dependencias
RUN npm install

# Exponer el puerto para el servidor Express
EXPOSE 3000

# Comando para iniciar el servidor
CMD ["node", "index.js"]
