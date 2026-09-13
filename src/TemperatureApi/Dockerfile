# temperature-api build, same multi-stage pattern as SmartHome.Api's Dockerfile.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY TemperatureApi.csproj ./
RUN dotnet restore TemperatureApi.csproj

COPY . .
RUN dotnet publish TemperatureApi.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

# Default port per the original task spec (README.md task 5.2)
ENV PORT=8081
EXPOSE 8081

ENTRYPOINT ["dotnet", "TemperatureApi.dll"]
