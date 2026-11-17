# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
# Mount points for persisted Data Protection keys and uploaded images
# (see docker-compose.yml).
RUN mkdir -p /app/DataProtection-Keys /app/uploads \
    && chown "$APP_UID" /app/DataProtection-Keys /app/uploads
EXPOSE 8080
EXPOSE 8081

# Environment variables for development.
# These can be overridden when running the container in production or when running from VS in regular mode (Default when not using the Debug configuration)
ENV ASPNETCORE_ENVIRONMENT=Development
# The connection string is not baked into the image. docker-compose passes it
# from `.env` (see `.env.example`).

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Central package versions and shared build settings are needed for restore.
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["solution/CleanArchitectureTemplate.Web/CleanArchitectureTemplate.Web.csproj", "solution/CleanArchitectureTemplate.Web/"]
COPY ["solution/CleanArchitectureTemplate.Infrastructure/CleanArchitectureTemplate.Infrastructure.csproj", "solution/CleanArchitectureTemplate.Infrastructure/"]
COPY ["solution/CleanArchitectureTemplate.ApplicationCore/CleanArchitectureTemplate.ApplicationCore.csproj", "solution/CleanArchitectureTemplate.ApplicationCore/"]
RUN dotnet restore "./solution/CleanArchitectureTemplate.Web/CleanArchitectureTemplate.Web.csproj"
COPY . .
WORKDIR "/src/solution/CleanArchitectureTemplate.Web"
RUN dotnet build "./CleanArchitectureTemplate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanArchitectureTemplate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
# Commit shown by VersionHelper (passed by CI: --build-arg GIT_COMMIT_HASH=...).
ARG GIT_COMMIT_HASH=unknown
ENV GIT_COMMIT_HASH=$GIT_COMMIT_HASH
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitectureTemplate.Web.dll"]
