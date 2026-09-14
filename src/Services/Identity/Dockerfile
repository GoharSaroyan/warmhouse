# Build context is the repo root (not this folder) - Identity.Api.csproj
# references the shared WarmHouse.WebDefaults project, which lives
# outside this directory.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY src/Services/Identity/Identity.Api.csproj src/Services/Identity/
COPY src/Common/WarmHouse.WebDefaults/WarmHouse.WebDefaults.csproj src/Common/WarmHouse.WebDefaults/
RUN dotnet restore src/Services/Identity/Identity.Api.csproj

COPY src/Services/Identity/ src/Services/Identity/
COPY src/Common/WarmHouse.WebDefaults/ src/Common/WarmHouse.WebDefaults/
RUN dotnet publish src/Services/Identity/Identity.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5007

ENTRYPOINT ["dotnet", "Identity.Api.dll"]
