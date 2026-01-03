FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Como o contexto já é a pasta RemessaSeguraBakend, o arquivo está na raiz
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# O Koyeb geralmente usa a porta 8000 ou 8080 por padrão
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RemessaSeguraBakend.dll"]