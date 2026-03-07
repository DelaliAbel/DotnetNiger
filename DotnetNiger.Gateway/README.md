# DotnetNiger.Gateway

API Gateway Ocelot pour la plateforme DotnetNiger.

## Stack

- .NET 8
- Ocelot (`Ocelot`)
- Swagger Aggregation (`MMLib.SwaggerForOcelot`)
- Ocelot cache (`Ocelot.Cache.CacheManager`)
- JWT Bearer auth
- Serilog

## Port et services

- Gateway: `http://localhost:5000`
- Identity downstream: `http://localhost:5075`
- Community downstream: `http://localhost:5269`

## Run local

```bash
dotnet restore
dotnet run
```

## Endpoints Gateway

- Swagger UI: `GET /swagger`
- Swagger docs aggregate:
  - `GET /swagger/docs/v1/identity`
  - `GET /swagger/docs/v1/community`
- Proxy health: `GET /health`
- Proxy info: `GET /info`

## Configuration

### 1) `ocelot.json`

Contient la configuration principale du gateway:

- Routes Identity et Community
- Authentication par route (`AuthenticationOptions`)
- Rate limiting Ocelot (`RateLimitOptions`)
- QoS Ocelot (`QoSOptions`)
- Cache Ocelot (`FileCacheOptions`)
- Swagger endpoints (`SwaggerEndPoints`)

### 2) `appsettings*.json`

Contient surtout:

- Logging levels
- JWT (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`)

### 3) `Program.cs`

Ne contient que le bootstrap:

- chargement `ocelot.json`
- enregistrement Ocelot + cache manager
- enregistrement JWT auth
- CORS
- SwaggerForOcelot UI
- `await app.UseOcelot()`

## Notes importantes

- Les routes custom du gateway ne sont pas gerees en `MapGet`; elles passent par Ocelot via `ocelot.json`.
- Si Swagger aggregate retourne une erreur, verifier d'abord que les services downstream sont demarres.
- L'ordre des services est important pour SwaggerForOcelot (`AddEndpointsApiExplorer` avant l'UI aggregate).
