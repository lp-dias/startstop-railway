# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo para dentro da imagem
COPY . .

# Restaura pacotes NuGet
RUN dotnet restore "StartStop.csproj"

# Publica em modo Release
RUN dotnet publish "StartStop.csproj" -c Release -o /app/publish

# Etapa final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia os arquivos publicados da etapa anterior
COPY --from=build /app/publish .

# Define o entrypoint da aplicação
ENTRYPOINT ["dotnet", "StartStop.dll"]
