# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY DeviceManagement.Api.csproj ./
RUN dotnet restore DeviceManagement.Api.csproj

COPY . .
RUN dotnet publish DeviceManagement.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5001

ENTRYPOINT ["dotnet", "DeviceManagement.Api.dll"]
