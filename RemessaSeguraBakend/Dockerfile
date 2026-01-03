# BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# O arquivo .csproj está na mesma pasta que o Dockerfile (Imagem 2)
COPY *.csproj ./
RUN dotnet restore

# Copia todo o conteúdo da pasta RemessaSeguraBakend
COPY . .
RUN dotnet publish -c Release -o /app/publish

# RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Configuração de porta para o Koyeb
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RemessaSeguraBakend.dll"]