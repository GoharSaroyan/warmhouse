# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Telemetry.Api.csproj ./
RUN dotnet restore Telemetry.Api.csproj

COPY . .
RUN dotnet publish Telemetry.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5006

ENTRYPOINT ["dotnet", "Telemetry.Api.dll"]
