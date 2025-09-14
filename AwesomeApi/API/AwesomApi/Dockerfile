FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app



FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Устанавливаем git и клонируем репозиторий
RUN apt-get update && apt-get install -y git \
    && git clone https://github.com/Gandoler/CaspLearn.git myrepo


# Переходим в каталог с проектом
WORKDIR /src/myrepo/AwesomeApi/API/AwesomApi


COPY AwesomApi.csproj ./
RUN dotnet restore "AwesomApi.csproj"
COPY . .
RUN dotnet build "AwesomApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AwesomApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Создаем необходимые директории
COPY API_files ./API_files


# Устанавливаем переменные окружения
ENV FILES_ROOT=/app/API_files/files
ENV ARCHIVES_DIR=/app/API_files/archive
ENV LOGS_ROOT=/app/API_files/logs

EXPOSE 5010

ENTRYPOINT ["dotnet", "AwesomApi.dll"]
