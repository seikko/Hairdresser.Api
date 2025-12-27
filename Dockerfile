# -------------------------
# Base stage
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# -------------------------
# Build stage
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the csproj first
COPY Hairdresser.Api/Hairdresser.Api/Hairdresser.Api.csproj Hairdresser.Api/
RUN dotnet restore Hairdresser.Api/Hairdresser.Api.csproj

# Copy the rest of the project
COPY Hairdresser.Api/ Hairdresser.Api/

# Build
WORKDIR /src/Hairdresser.Api
RUN dotnet build ./Hairdresser.Api.csproj -c $BUILD_CONFIGURATION -o /app/build

# -------------------------
# Publish stage
# -------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish ./Hairdresser.Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------------------------
# Final / Runtime stage
# -------------------------
FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Hairdresser.Api.dll"]
