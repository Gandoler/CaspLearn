FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Устанавливаем git и клонируем репозиторий
RUN apt-get update && apt-get install -y git \
    && git clone https://github.com/Gandoler/CaspLearn.git myrepo 


# Переходим в каталог с проектом
WORKDIR /AwesomeApi/API/AwesomApi


RUN dotnet restore "AwesomApi/AwesomApi.csproj"
COPY . .
WORKDIR "/src/AwesomApi"
RUN dotnet build "AwesomApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AwesomApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем переменные окружения для базы данных

ENV FILES_ROOT = "E:/API/files/files"
ENV ARCHIVES_DIR = "E:/API/archive"
ENV LOGS_ROOT = "E:/API/logs "


ENTRYPOINT ["dotnet", "AwesomApi.dll"]
