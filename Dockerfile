# Usa la imagen de SDK de .NET para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia los archivos .csproj y restaura las dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia el resto de los archivos y publica el proyecto
COPY . ./
RUN dotnet publish -c Release -o out

# Usa la imagen de runtime de .NET para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

# Establece el puerto de la aplicación
ENV ASPNETCORE_URLS=http://+:80

# Expone los puertos 80 y 443 (si deseas HTTPS)
EXPOSE 80


# Define el comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "jejames.api.ApiFactura.dll"]
