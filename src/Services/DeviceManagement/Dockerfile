# Build context is the repo root (not this folder) - DeviceManagement.Api.csproj
# references the shared WarmHouse.WebDefaults project, which lives
# outside this directory.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY src/Services/DeviceManagement/DeviceManagement.Api.csproj src/Services/DeviceManagement/
COPY src/Common/WarmHouse.WebDefaults/WarmHouse.WebDefaults.csproj src/Common/WarmHouse.WebDefaults/
RUN dotnet restore src/Services/DeviceManagement/DeviceManagement.Api.csproj

COPY src/Services/DeviceManagement/ src/Services/DeviceManagement/
COPY src/Common/WarmHouse.WebDefaults/ src/Common/WarmHouse.WebDefaults/
RUN dotnet publish src/Services/DeviceManagement/DeviceManagement.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5001

ENTRYPOINT ["dotnet", "DeviceManagement.Api.dll"]
