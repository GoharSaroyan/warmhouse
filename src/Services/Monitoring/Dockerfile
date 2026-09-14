# Build context is the repo root (not this folder) - Monitoring.Api.csproj
# references the shared WarmHouse.Contracts project, which lives outside
# this directory.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY src/Services/Monitoring/Monitoring.Api.csproj src/Services/Monitoring/
COPY src/Common/WarmHouse.Contracts/WarmHouse.Contracts.csproj src/Common/WarmHouse.Contracts/
RUN dotnet restore src/Services/Monitoring/Monitoring.Api.csproj

COPY src/Services/Monitoring/ src/Services/Monitoring/
COPY src/Common/WarmHouse.Contracts/ src/Common/WarmHouse.Contracts/
RUN dotnet publish src/Services/Monitoring/Monitoring.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5005

ENTRYPOINT ["dotnet", "Monitoring.Api.dll"]
