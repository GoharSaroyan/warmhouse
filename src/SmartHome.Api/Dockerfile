# Translated from apps/smart_home/Dockerfile (Go multi-stage build ->
# .NET multi-stage build: SDK to publish, ASP.NET runtime to run).

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY SmartHome.Api.csproj ./
RUN dotnet restore SmartHome.Api.csproj

COPY . .
RUN dotnet publish SmartHome.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Command to run the application
ENTRYPOINT ["dotnet", "SmartHome.Api.dll"]
