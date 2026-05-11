# DotnetNiger

Open-source community platform built with .NET microservices, Ocelot API Gateway, and OpenIddict authentication.

A modern, modular platform for the .NET Niger community — featuring posts, events, resources, member profiles, newsletters, and multi-tenant administration.

## Architecture Overview

```
Client (Browser / Mobile / API)
        │
        ▼
┌─────────────────────┐    Port 5000
│   API Gateway       │   (Ocelot)
│   DotnetNiger       │   Routing, Auth, Rate-Limiting,
│   .Gateway          │   QoS, Swagger Aggregation
└────────┬────────────┘
         │
    ┌────┴────┐
    ▼         ▼
┌────────┐ ┌──────────┐
│Identity│ │ Community │
│ :5075  │ │ :5269     │
│ (:8081)│ │ (:8082)   │
└────────┘ └──────────┘
```

All external traffic goes through the **Gateway** (port `5000`). Downstream services are not exposed directly in production.

## Services

| Service | Dev Port | Container Port | Description |
|---------|----------|----------------|-------------|
| **Gateway** | `5000` | `5000` | Ocelot API Gateway — routing, JWT auth, rate limiting, health checks, Swagger merge |
| **Identity** | `5075` | `8081` | Authentication, multi-tenant users, roles, permissions, OAuth2/OIDC (OpenIddict) |
| **Community** | `5269` | `8082` | Community content — posts, events, resources, comments, newsletters, member profiles, search |

## Quick Start

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
dotnet restore
```

Start services in order (3 terminals):

```bash
# Terminal 1 — Identity
cd DotnetNiger.Identity
dotnet run

# Terminal 2 — Community
cd DotnetNiger.Community
dotnet run

# Terminal 3 — Gateway
cd DotnetNiger.Gateway
dotnet run
```

## Key URLs

| URL | Description |
|-----|-------------|
| `http://localhost:5000/swagger` | Aggregated Swagger UI (all services) |
| `http://localhost:5000/health` | Gateway health |
| `http://localhost:5000/health/downstream` | All downstream services health |
| `http://localhost:5000/health/services` | Registered services configuration |
| `http://localhost:5000/metrics/latency` | Endpoint latency metrics |
| `http://localhost:5075/swagger` | Identity Swagger (direct) |
| `http://localhost:5269/swagger` | Community Swagger (direct) |

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 9.0 |
| Gateway | Ocelot 24.x, MMLib.SwaggerForOcelot, Polly, CacheManager |
| Identity | ASP.NET Core Identity, OpenIddict 5.x, EF Core + SQLite |
| Community | ASP.NET Core, EF Core + SQLite, JWT Bearer |
| Auth | JWT (symmetric key) + OpenIddict OIDC |
| Logging | Serilog (Console + File) |
| Email | MailKit (SMTP) |
| Social Login | Google, Microsoft, GitHub OAuth |
| Container | Docker, docker-compose |

## Configuration Essentials

| Variable | Purpose |
|----------|---------|
| `Jwt:Key` | Shared JWT signing key (min 32 chars) |
| `Jwt:Issuer` | JWT issuer |
| `Jwt:Audience` | JWT audience |
| `IdentityApi:BaseUrl` | Identity URL used by Community for internal calls |
| `Admin:ApiKey` | Admin API key for Community |

## Documentation

- [docs/INDEX.md](docs/INDEX.md)
- [docs/SETUP.md](docs/SETUP.md)
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- [docs/API.md](docs/API.md)
- [DotnetNiger.Gateway/README.md](DotnetNiger.Gateway/README.md)
- [DotnetNiger.Identity/README.md](DotnetNiger.Identity/README.md)
- [DotnetNiger.Identity/INTEGRATION_GUIDE.md](DotnetNiger.Identity/INTEGRATION_GUIDE.md)
- [DotnetNiger.Community/README.md](DotnetNiger.Community/README.md)
- [DotnetNiger.Community/INTEGRATION_GUIDE.md](DotnetNiger.Community/INTEGRATION_GUIDE.md)

## License

MIT License — see [LICENSE.md](LICENSE.md)
