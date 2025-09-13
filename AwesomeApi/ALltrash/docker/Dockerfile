# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/AwesomeFiles.Api/AwesomeFiles.Api.csproj", "src/AwesomeFiles.Api/"]
COPY ["src/AwesomeFiles.Common/AwesomeFiles.Common.csproj", "src/AwesomeFiles.Common/"]

# Restore dependencies
RUN dotnet restore "src/AwesomeFiles.Api/AwesomeFiles.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/AwesomeFiles.Api"
RUN dotnet build "AwesomeFiles.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AwesomeFiles.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Create directories for files and archives
RUN mkdir -p /app/files /app/archives

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV FILES_ROOT=/app/files
ENV ARCHIVES_DIR=/app/archives

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AwesomeFiles.Api.dll"]
