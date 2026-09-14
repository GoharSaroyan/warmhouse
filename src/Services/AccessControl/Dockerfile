# Build context is the repo root (not this folder) - AccessControl.Api.csproj
# references the shared WarmHouse.Contracts project, which lives outside
# this directory.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY src/Services/AccessControl/AccessControl.Api.csproj src/Services/AccessControl/
COPY src/Common/WarmHouse.Contracts/WarmHouse.Contracts.csproj src/Common/WarmHouse.Contracts/
RUN dotnet restore src/Services/AccessControl/AccessControl.Api.csproj

COPY src/Services/AccessControl/ src/Services/AccessControl/
COPY src/Common/WarmHouse.Contracts/ src/Common/WarmHouse.Contracts/
RUN dotnet publish src/Services/AccessControl/AccessControl.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5004

ENTRYPOINT ["dotnet", "AccessControl.Api.dll"]
