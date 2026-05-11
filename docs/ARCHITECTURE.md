# Architecture

## Overview

DotnetNiger follows a **microservices architecture** with an **API Gateway** as the single entry point. All client traffic flows through the Gateway, which handles routing, authentication, rate limiting, and Swagger aggregation.

```
                    ┌─────────────┐
                    │   Client    │
                    │ (Browser /  │
                    │  Mobile /   │
                    │  API)       │
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │   Gateway   │  Port 5000
                    │  (Ocelot)   │
                    │  .NET 9.0   │
                    └──────┬──────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
       ┌──────▼──────┐ ┌──▼──────┐ ┌──▼──────────┐
       │  Identity   │ │Community│ │  Future      │
       │  :5075      │ │ :5269   │ │  Services    │
       │  (:8081)    │ │ (:8082) │ │  (...)       │
       │  OpenIddict │ │ JWT     │ │              │
       │  OIDC       │ │ Bearer  │ │              │
       └──────┬──────┘ └─────────┘ └─────────────┘
              │
       ┌──────▼──────┐
       │  SQLite     │
       │  (EF Core)  │
       └─────────────┘
```

## Service Responsibilities

### Gateway (`DotnetNiger.Gateway`)

- **Single entry point** — all external requests go through port `5000`
- **Routing** — Ocelot maps upstream paths to downstream services
- **Authentication** — Validates JWT tokens before forwarding to downstream services
- **Rate Limiting** — Per-route rate limits (configurable in route JSON files)
- **QoS (Quality of Service)** — Circuit breaker via Polly, request timeouts
- **Caching** — Response caching for read-heavy endpoints (posts, events, resources)
- **Swagger Aggregation** — Merges all downstream Swagger docs into a single UI
- **Health Checks** — `/health`, `/health/downstream`, `/health/ready`, `/health/services`
- **Latency Metrics** — `/metrics/latency` endpoint with P50/P95/P99 percentiles
- **Request Tracing** — `X-Request-ID` header propagation and logging

### Identity (`DotnetNiger.Identity`)

- **Authentication** — Register, login, email confirmation, social login (Google, Microsoft, GitHub)
- **OAuth2 / OIDC** — OpenIddict-based token endpoint (`/connect/token`) with password and refresh token grants
- **Multi-Tenancy** — Tenant isolation via `TenantId` claim in JWT
- **User Management** — CRUD users, roles, permissions, API keys
- **Admin** — Super-admin bootstrap, platform stats, tenant management
- **Profile** — Get/update/delete own profile

### Community (`DotnetNiger.Community`)

- **Posts** — CRUD with pagination, searchable
- **Events** — CRUD with registration system, publish/unpublish workflow
- **Resources** — CRUD with view counting, categorized
- **Comments** — Nested comments on posts and events
- **Newsletters** — Subscribe, verify, unsubscribe with admin management
- **Member Profiles** — Profile, social links within the community
- **Admin** — Dashboard stats, user/role/permission management, event moderation
- **Search** — Full-text search across posts, events, and resources
- **Tags & Categories** — Content classification
- **Projects & Partners** — Community directory

## Dependency Rules

The key architectural constraint enforced by tests is:

> **Application layer must NOT reference Infrastructure.Repositories directly**

### Enforcement

Architecture tests are in `DotnetNiger.Tests/DotnetNiger.Architecture.Tests/` and verify:
- Application projects only depend on Domain and Abstractions
- Infrastructure implements abstractions defined in Application
- No circular dependencies between projects

### Clean Architecture Layer Structure

```
Api/Controllers/        → REST endpoints
Api/Middleware/         → Error handling, custom middleware
Api/ServiceExtensions   → DI registration extension methods
Application/DTOs/       → Request/Response data transfer objects
Application/Services/   → Business logic
Domain/Entities/        → EF Core entities
Infrastructure/         → DbContext, repositories, seeding
```

## Communication Between Services

- **Gateway → all**: HTTP routing via Ocelot configuration
- **Community → Identity**: Typed HTTP client (`IIdentityApiClient`) for user/role provisioning
- **Identity → Community**: Inter-service calls for user data synchronization

## Dynamic Service Discovery

The Gateway uses a config-driven approach for multi-service support:

1. Each service has a JSON route file: `ocelot.<service>.routes.json`
2. Each service is registered in `appsettings.json` under `DownstreamServices`
3. At startup, the Gateway merges all route files into a single `ocelot.json`
4. Health checks, Swagger merging, and container host rewriting are fully dynamic

To add a new service, simply:
1. Create `ocelot.newservice.routes.json`
2. Add `DownstreamServices:NewService` configuration
3. Add the container in `docker-compose.yml`

## Health & Observability

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Gateway liveness probe |
| `GET /health/ready` | Readiness probe (all downstreams must respond) |
| `GET /health/downstream` | Detailed health of all downstream services |
| `GET /health/services` | Registered service configuration |
| `GET /metrics/latency` | Per-endpoint latency (P50/P95/P99) |

All services use **Serilog** for structured logging (console + rolling file).

## Technology Map

| Area | Technology |
|------|-----------|
| Runtime | .NET 9.0 |
| API Gateway | Ocelot 24.x |
| Auth | ASP.NET Core Identity + OpenIddict 5.x |
| OAuth Providers | Google, Microsoft, GitHub |
| ORM | Entity Framework Core 9.x |
| Database | SQLite (dev) |
| Validation | FluentValidation (Identity) |
| API Docs | Swashbuckle / Swagger |
| Logging | Serilog |
| Resilience | Polly (via Ocelot) |
| Caching | Ocelot CacheManager |
| Container | Docker, docker-compose |
| Email | MailKit (SMTP) |
