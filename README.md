# WarmHouse (C#)

A direct C# translation of the Go project at
[Yandex-Practicum/architecture-warmhouse](https://github.com/Yandex-Practicum/architecture-warmhouse)
(cloned locally at `../warmehousego`). Same two-service architecture, same
database schema, same REST API - only the language changed.

## What this is

The original repo's `apps/smart_home` is a Go/Gin monolith that manages
sensors in Postgres and calls out to a `temperature-api` service over HTTP
for live readings (the `temperature-api` app itself was left as an exercise
in the Go repo's task list - it's implemented here too, in C#, so the whole
system is one language).

| Go source | C# equivalent |
|---|---|
| `apps/smart_home/main.go` | [`src/SmartHome.Api/Program.cs`](src/SmartHome.Api/Program.cs) |
| `apps/smart_home/models/sensor.go` | [`src/SmartHome.Api/Models/`](src/SmartHome.Api/Models/) |
| `apps/smart_home/db/db.go` (pgx) | [`src/SmartHome.Api/Data/SensorRepository.cs`](src/SmartHome.Api/Data/SensorRepository.cs) (Npgsql) |
| `apps/smart_home/handlers/sensors.go` (gin) | [`src/SmartHome.Api/Controllers/SensorsController.cs`](src/SmartHome.Api/Controllers/SensorsController.cs) (ASP.NET Core MVC) |
| `apps/smart_home/services/temperature_service.go` | [`src/SmartHome.Api/Services/TemperatureService.cs`](src/SmartHome.Api/Services/TemperatureService.cs) |
| `apps/smart_home/init.sql` | [`db/init.sql`](db/init.sql) (unchanged) |
| *(task 5.1 - a temperature-api to write yourself)* | [`src/TemperatureApi/Program.cs`](src/TemperatureApi/Program.cs) |
| `apps/docker-compose.yml` | [`docker-compose.yml`](docker-compose.yml) (postgres/temperature-api sections filled in) |

Same database, same tables, same REST routes, same JSON shapes
(`snake_case` field names preserved via `[JsonPropertyName]`), same
environment variables (`DATABASE_URL`, `TEMPERATURE_API_URL`, `PORT`), same
default ports (8080 for the app, 8081 for temperature-api).

## Prerequisites

- Docker and Docker Compose, **or**
- .NET 9 SDK (for running the two projects directly)

## Getting Started

### Option 1: Using Docker Compose (Recommended)

```bash
./init.sh
```

or directly:

```bash
docker-compose up -d --build
```

The API will be available at http://localhost:8080, the temperature service
at http://localhost:8081. (If 8081 is already taken by something else on
your machine, change the host side of the `temperature-api` port mapping in
`docker-compose.yml`, e.g. `"8091:8081"` - the container-to-container URL
`http://temperature-api:8081` used by `app` is unaffected either way.)

### Option 2: Manual setup

```bash
docker-compose up -d postgres
dotnet run --project src/TemperatureApi
dotnet run --project src/SmartHome.Api
```

## API Testing

Import `smarthome-api.postman_collection.json` into Postman, same as the
original repo.

## API Endpoints (unchanged from the Go version)

- `GET /health` - Health check
- `GET /api/v1/sensors` - Get all sensors
- `GET /api/v1/sensors/:id` - Get a specific sensor
- `POST /api/v1/sensors` - Create a new sensor
- `PUT /api/v1/sensors/:id` - Update a sensor
- `DELETE /api/v1/sensors/:id` - Delete a sensor
- `PATCH /api/v1/sensors/:id/value` - Update a sensor's value and status
- `GET /api/v1/sensors/temperature/:location` - Fetch live temperature for a location
