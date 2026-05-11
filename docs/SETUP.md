# Setup Guide

## Prerequisites

- .NET SDK 9.0+
- Node.js + npm (for frontend tools / scripts)
- Git
- Docker + docker-compose (optional, for containerized deployment)

## Clone & Restore

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
dotnet restore DotnetNiger.slnx
```

## Local Development (3 Terminals)

Start services in this order: **Identity → Community → Gateway**.

### Terminal 1 — Identity Service

```bash
cd DotnetNiger.Identity
dotnet run
```

Runs on `http://localhost:5075`. The database is auto-created (SQLite). A super admin is seeded on first run:
- Email: `admin@dotnetniger.com`
- Password: `Admin@123456`

Swagger: `http://localhost:5075/swagger`

### Terminal 2 — Community Service

```bash
cd DotnetNiger.Community
dotnet run
```

Runs on `http://localhost:5269`. Depends on Identity being running for JWT validation.

Swagger: `http://localhost:5269/swagger`

### Terminal 3 — Gateway Service

```bash
cd DotnetNiger.Gateway
dotnet run
```

Runs on `http://localhost:5000`. The Ocelot configuration is auto-generated at startup by merging:
- `ocelot.global.json` (global settings)
- `ocelot.identity.routes.json` (Identity routes)
- `ocelot.community.routes.json` (Community routes)

Swagger (aggregated): `http://localhost:5000/swagger`

## Docker Deployment

```bash
docker-compose up --build
```

This starts all three services with their container ports:
- Gateway: `localhost:5000`
- Identity: `localhost:8081`
- Community: `localhost:8082`

## Build & Test

```bash
# Build all projects
dotnet build DotnetNiger.slnx

# Run tests
dotnet test DotnetNiger.slnx

# Quick CI-equivalent
dotnet restore DotnetNiger.slnx
dotnet build DotnetNiger.slnx --configuration Release --no-restore
dotnet test DotnetNiger.slnx --configuration Release --no-build
```

## Configuration Variables

### JWT (required — must match across all services)

```json
{
  "Jwt": {
    "Key": "YourSecretKeyMin32CharactersLong!",
    "Issuer": "DotnetNiger.Identity",
    "Audience": "DotnetNiger.Identity.Client"
  }
}
```

### SMTP (optional — for email confirmation)

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your@email.com",
    "Password": "your-password",
    "FromEmail": "noreply@dotnetniger.com",
    "FromName": "DotnetNiger",
    "AppBaseUrl": "http://localhost:5075"
  }
}
```

If `Host` is empty, emails are logged to console and confirmation codes are returned directly in API responses.

### Social Login (optional)

```json
{
  "Authentication": {
    "Google": { "ClientId": "...", "ClientSecret": "..." },
    "Microsoft": { "ClientId": "...", "ClientSecret": "..." },
    "GitHub": { "ClientId": "...", "ClientSecret": "..." }
  }
}
```

Providers are only activated when their `ClientId` is non-empty.

### User Secrets (for sensitive data)

```bash
cd DotnetNiger.Identity
dotnet user-secrets set "Smtp:Password" "your-password"
dotnet user-secrets set "Authentication:Google:ClientId" "your-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-secret"
```

### Adding a New Service

1. Add `ocelot.<service>.routes.json` in the Gateway project
2. Add a `DownstreamServices:<Service>` section in `appsettings.json`
3. Add the service container in `docker-compose.yml`
4. Create the service project following Identity/Community patterns

The Gateway dynamically discovers all services from configuration and merges their routes, health checks, and Swagger docs at startup.
