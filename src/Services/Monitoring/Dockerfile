# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Monitoring.Api.csproj ./
RUN dotnet restore Monitoring.Api.csproj

COPY . .
RUN dotnet publish Monitoring.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5005

ENTRYPOINT ["dotnet", "Monitoring.Api.dll"]
