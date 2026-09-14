# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY AccessControl.Api.csproj ./
RUN dotnet restore AccessControl.Api.csproj

COPY . .
RUN dotnet publish AccessControl.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5004

ENTRYPOINT ["dotnet", "AccessControl.Api.dll"]
