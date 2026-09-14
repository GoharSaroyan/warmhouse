# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Heating.Api.csproj ./
RUN dotnet restore Heating.Api.csproj

COPY . .
RUN dotnet publish Heating.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5002

ENTRYPOINT ["dotnet", "Heating.Api.dll"]
