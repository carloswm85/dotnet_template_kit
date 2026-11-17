# Multi-stage build for the API. Build context: the solution root
# (see docker-compose.yml).

# Runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
# Mount point for persisted Data Protection keys (see docker-compose.yml).
RUN mkdir -p /app/DataProtection-Keys \
    && chown "$APP_UID" /app/DataProtection-Keys
EXPOSE 8080
# The connection string and JWT settings are not baked into the image.
# docker-compose passes them from `.env` (see `.env.example`).

# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Central package versions and shared build settings are needed for restore.
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["solution/CleanArchitectureTemplate.API/CleanArchitectureTemplate.API.csproj", "solution/CleanArchitectureTemplate.API/"]
COPY ["solution/CleanArchitectureTemplate.Infrastructure/CleanArchitectureTemplate.Infrastructure.csproj", "solution/CleanArchitectureTemplate.Infrastructure/"]
COPY ["solution/CleanArchitectureTemplate.ApplicationCore/CleanArchitectureTemplate.ApplicationCore.csproj", "solution/CleanArchitectureTemplate.ApplicationCore/"]
RUN dotnet restore "./solution/CleanArchitectureTemplate.API/CleanArchitectureTemplate.API.csproj"
COPY . .
WORKDIR "/src/solution/CleanArchitectureTemplate.API"
RUN dotnet build "./CleanArchitectureTemplate.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanArchitectureTemplate.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitectureTemplate.API.dll"]