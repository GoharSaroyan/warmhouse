# WarmHouse Microservices

Target microservice architecture for the "Тёплый дом" (WarmHouse) smart
home platform — see [PROJECT_TEMPLATE.md](PROJECT_TEMPLATE.md) and
[docs/c4](docs/c4) for the design.

## Prerequisites

- Docker and Docker Compose (or the .NET 9 SDK, to run services individually)

## Getting Started

```bash
docker-compose up -d --build
```

This starts:

- **postgres** — one Postgres instance, one database per service (see
  [db/init.sql](db/init.sql))
- **rabbitmq** — message broker for async events between services
  (management UI at http://localhost:15672, guest/guest)
- Each microservice (Device Management, Heating, Lighting, Access
  Control, Monitoring, Device Gateway, Telemetry, Identity, Billing)
- **api-gateway** — single entry point at http://localhost:5000, routing
  to the services above

## API Testing

A Postman collection is provided: `smarthome-api.postman_collection.json`.

## API Endpoints (via the API Gateway, http://localhost:5000)

- `/api/v1/devices/*` → Device Management Service
- `/api/v1/heating/*` → Heating Service
- `/api/v1/lighting/*` → Lighting Service
- `/api/v1/access/*` → Access Control Service
- `/api/v1/monitoring/*` → Monitoring Service
- `/api/v1/telemetry/*` → Telemetry Service
- `/api/v1/identity/*` → Identity Service
- `/api/v1/billing/*` → Billing Service

Each service also exposes Swagger directly on its own port (see
`docker-compose.yml`) when running in Development.
