# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the csproj first
COPY Hairdresser.Api/Hairdresser.Api/Hairdresser.Api.csproj Hairdresser.Api/
RUN dotnet restore Hairdresser.Api/Hairdresser.Api.csproj

# Copy the rest of the project
COPY Hairdresser.Api/ Hairdresser.Api/

# Build
WORKDIR /src/Hairdresser.Api
RUN dotnet build ./Hairdresser.Api.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish ./Hairdresser.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Hairdresser.Api.dll"]
