# DotnetNiger.Identity

Service Identity pour l'authentification et la gestion des utilisateurs.

Derniere mise a jour: 2026-03-14

## Demarrage rapide

```bash
dotnet restore
cd DotnetNiger.Identity
dotnet ef database update
dotnet run
```

Swagger: <http://localhost:5075/swagger>

## Endpoints importants

- POST /api/v1/auth/login
- POST /api/v1/auth/register
- GET /api/v1/me
- PUT /api/v1/me
- GET /api/v1/admin/users
- GET /api/v1/diagnostics/ping
- GET /api/v1/diagnostics/health

## Build

Le warning NU1900 est neutralise dans le projet Identity pour les environnements reseau restreints.

## Tests

```bash
dotnet test DotnetNiger.Identity.Tests
dotnet test DotnetNiger.Identity.IntegrationTests
```
