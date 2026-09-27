# ---------- Сборка ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала только csproj — слой кэшируется, пока не менялись зависимости
COPY ["ITSchoolCRM.API.csproj", "./"]
RUN dotnet restore "ITSchoolCRM.API.csproj"

COPY . .

# Публикация С указанием RID (-r linux-x64):
# без RID нативные ассеты SkiaSharp (libSkiaSharp.so) могут не попасть
# в выходную папку — тогда на Linux будет DllNotFoundException при
# экспорте. UseAppHost=false оставляет развёртывание framework-dependent.
RUN dotnet publish "ITSchoolCRM.API.csproj" -c Release -r linux-x64 \
    -o /app/publish /p:UseAppHost=false

# ---------- Запуск ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# В образах .NET приложение по умолчанию слушает порт 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Системные зависимости:
#   curl       — для healthcheck из docker-compose
#   fontconfig — нужен SkiaSharp/QuestPDF при отрисовке (экспорт PDF/xlsx
#                с картинками/диаграммами падает без него)
#   libfontconfig1 — нативная библиотека fontconfig
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
       curl \
       fontconfig \
       libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "ITSchoolCRM.API.dll"]