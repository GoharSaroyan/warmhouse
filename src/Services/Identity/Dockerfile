# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Identity.Api.csproj ./
RUN dotnet restore Identity.Api.csproj

COPY . .
RUN dotnet publish Identity.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5007

ENTRYPOINT ["dotnet", "Identity.Api.dll"]
