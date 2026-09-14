# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Gateway.Api.csproj ./
RUN dotnet restore Gateway.Api.csproj

COPY . .
RUN dotnet publish Gateway.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
