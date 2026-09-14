# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder
WORKDIR /src

COPY Billing.Api.csproj ./
RUN dotnet restore Billing.Api.csproj

COPY . .
RUN dotnet publish Billing.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 5008

ENTRYPOINT ["dotnet", "Billing.Api.dll"]
