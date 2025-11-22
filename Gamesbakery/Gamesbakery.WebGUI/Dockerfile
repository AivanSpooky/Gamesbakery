# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore (multi-stage optimization)
COPY ["Gamesbakery.WebGUI/Gamesbakery.WebGUI.csproj", "Gamesbakery.WebGUI/"]
COPY ["Gamesbakery.BusinessLogic/Gamesbakery.BusinessLogic.csproj", "Gamesbakery.BusinessLogic/"]
COPY ["Gamesbakery.Core/Gamesbakery.Core.csproj", "Gamesbakery.Core/"]
COPY ["Gamesbakery.DataAccess/Gamesbakery.DataAccess.csproj", "Gamesbakery.DataAccess/"]
COPY ["Gamesbakery.Infrastructure/Gamesbakery.Infrastructure.csproj", "Gamesbakery.Infrastructure/"]
RUN dotnet restore "./Gamesbakery.WebGUI/Gamesbakery.WebGUI.csproj" /p:UseRazorBuildServer=false

# Copy source code
COPY . .
WORKDIR "/src/Gamesbakery.WebGUI"
RUN dotnet build "./Gamesbakery.WebGUI.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:UseRazorBuildServer=false

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gamesbakery.WebGUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:UseRazorBuildServer=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl and netcat for health checks
USER root
RUN apt-get update && \
    apt-get install -y curl netcat-openbsd && \
    rm -rf /var/lib/apt/lists/* && \
    mkdir -p /app/dataprotection-keys /app/keys /app/logs && \
    chmod 777 /app/dataprotection-keys /app/keys /app/logs && \
    chown -R 1000:1000 /app/dataprotection-keys /app/keys /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user (optional, for security)
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app
USER appuser

EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:80/health/check || exit 1

USER 1000
ENTRYPOINT ["dotnet", "Gamesbakery.WebGUI.dll"]