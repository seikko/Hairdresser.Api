# -------------------------
# Build stage
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the csproj first (for faster restore caching)
COPY BookingAPI/BookingAPI.csproj BookingAPI/
RUN dotnet restore BookingAPI/BookingAPI.csproj

# Copy the rest of the project
COPY BookingAPI/ BookingAPI/

# Build
WORKDIR /src/BookingAPI
RUN dotnet build -c Release -o /app/build

# -------------------------
# Publish stage
# -------------------------
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# -------------------------
# Runtime stage
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Expose port for App Platform / container
EXPOSE 80

# Ensure Kestrel listens on all interfaces
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Optional: run as non-root (sağlık için)
# ARG APP_UID=1000
# RUN adduser -u $APP_UID -D appuser
# USER appuser

ENTRYPOINT ["dotnet", "BookingAPI.dll"]
