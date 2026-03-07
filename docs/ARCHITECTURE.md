# Architecture DotnetNiger

Vue d'ensemble actuelle du projet DotnetNiger (microservices .NET 8) avec API Gateway Ocelot.

## Topologie

```text
Client
  |
  v
Gateway (DotnetNiger.Gateway, Ocelot) :5000
  |------------------------------|
  v                              v
Identity Service :5075      Community Service :5269
```

## Roles des services

### Gateway (`DotnetNiger.Gateway`)

- Point d'entree unique
- Routage HTTP via Ocelot (`ocelot.json`)
- Validation JWT Bearer
- Rate limiting (Ocelot)
- QoS/circuit breaker/timeouts (Ocelot)
- Cache de reponse par route (Ocelot)
- Aggregation Swagger (Identity + Community)

### Identity (`DotnetNiger.Identity`)

- Authentification et autorisation
- Gestion utilisateurs, roles, permissions, tokens
- Endpoints diagnostics/admin

### Community (`DotnetNiger.Community`)

- Endpoints communautaires: posts, comments, events, resources, etc.

## Principes de configuration

- Le bootstrap technique est dans `Program.cs` de chaque service.
- Les regles de proxy Gateway sont centralisees dans `DotnetNiger.Gateway/ocelot.json`.
- Les secrets/environnements sont geres via `appsettings*.json` et variables d'environnement.

## Gateway Ocelot: elements cles

Dans `ocelot.json`:

- `Routes`: upstream/downstream mapping
- `AuthenticationOptions`: protection Bearer route par route
- `RateLimitOptions`: quotas par route
- `QoSOptions`: resilience par route
- `FileCacheOptions`: cache route par route
- `SwaggerEndPoints`: sources swagger downstream
- `GlobalConfiguration`: base URL, request id, options globales

## Observabilite

- Logging: Serilog
- Correlation: `X-Request-ID` au niveau gateway
- Health check: route gateway `/health` proxifiee vers Identity

## Evolution

Une implementation YARP existe dans `DotnetNiger.Gateway.Yarp`, mais le gateway actif documente ici est `DotnetNiger.Gateway` base sur Ocelot.
