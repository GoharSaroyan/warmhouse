# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Lighting.Api.csproj ./
RUN dotnet restore Lighting.Api.csproj

COPY . .
RUN dotnet publish Lighting.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5003

ENTRYPOINT ["dotnet", "Lighting.Api.dll"]
