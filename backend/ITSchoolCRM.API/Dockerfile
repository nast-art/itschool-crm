# ---------- Сборка ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Сначала только csproj — слой кэшируется, пока не менялись зависимости
COPY ["ITSchoolCRM.API.csproj", "./"]
RUN dotnet restore "ITSchoolCRM.API.csproj"

COPY . .
RUN dotnet publish "ITSchoolCRM.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Запуск ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# В образах .NET 8 приложение по умолчанию слушает порт 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# curl нужен для healthcheck из docker-compose
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "ITSchoolCRM.API.dll"]