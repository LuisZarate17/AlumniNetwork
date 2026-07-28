# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (cached unless the csproj changes)
COPY CougarConnect/CougarConnect/CougarConnect.csproj CougarConnect/CougarConnect/
RUN dotnet restore CougarConnect/CougarConnect/CougarConnect.csproj

# Copy the rest of the web project and publish
COPY CougarConnect/CougarConnect/ CougarConnect/CougarConnect/
RUN dotnet publish CougarConnect/CougarConnect/CougarConnect.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Bind to the port the host provides (Render injects $PORT), defaulting to 8080 locally.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet CougarConnect.dll"]
