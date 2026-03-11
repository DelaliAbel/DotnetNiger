# DotnetNiger.Gateway

API Gateway Ocelot pour la plateforme DotnetNiger.

Derniere mise a jour: 2026-07-14

## Stack

- .NET 8
- Ocelot
- MMLib.SwaggerForOcelot (swagger aggregate)
- Ocelot.Cache.CacheManager (DictionaryHandle)
- JWT Bearer auth
- Serilog

## Ports

- Gateway: `http://localhost:5000`
- Identity downstream: `http://localhost:5075`
- Community downstream: `http://localhost:5269`

## Demarrage

```bash
dotnet restore
dotnet run
```

## Endpoints Gateway

| Route          | Description                   |
| -------------- | ----------------------------- |
| `GET /swagger` | Swagger UI aggregate          |
| `GET /health`  | Health check (proxy Identity) |
| `GET /info`    | Info service (cache 30s)      |

## Table des routes Ocelot (`ocelot.json`)

### Identity

| Route Ocelot                          | Downstream                               | Auth                       |
| ------------------------------------- | ---------------------------------------- | -------------------------- |
| `GET /health`                         | `:5075/api/v1/health`                    | Publique                   |
| `GET /info`                           | `:5075/api/v1/health`                    | Publique (cache 30s)       |
| `* /api/v1/auth/{everything}`         | `:5075/api/v1/auth/{everything}`         | Publique (rate: 30/min)    |
| `* /api/v1/users/{everything}`        | `:5075/api/v1/users/{everything}`        | JWT Bearer (rate: 100/min) |
| `* /api/v1/tokens/{everything}`       | `:5075/api/v1/tokens/{everything}`       | JWT Bearer                 |
| `* /api/v1/social-links/{everything}` | `:5075/api/v1/social-links/{everything}` | JWT Bearer                 |
| `* /api/v1/roles/{everything}`        | `:5075/api/v1/roles/{everything}`        | JWT Bearer                 |
| `* /api/v1/permissions/{everything}`  | `:5075/api/v1/permissions/{everything}`  | JWT Bearer                 |
| `* /api/v1/api-keys/{everything}`     | `:5075/api/v1/api-keys/{everything}`     | JWT Bearer                 |
| `* /api/v1/admin/{everything}`        | `:5075/api/v1/admin/{everything}`        | JWT Bearer                 |
| `* /api/v1/diagnostics/{everything}`  | `:5075/api/v1/diagnostics/{everything}`  | Publique                   |

### Community — contenu public (GET uniquement)

14 routes publiques pour: posts, comments, events, projects, resources, categories, tags, partners, team, stats, search.

Exemple pattern: `GET /api/community/posts/{everything}` → `:5269/api/posts/{everything}` (pas d'auth)

### Community — contenu prive (write)

14 routes protegees pour les memes ressources (POST/PUT/DELETE/PATCH).

Exemple pattern: `POST|PUT|DELETE|PATCH /api/community/posts/{everything}` → `:5269/api/posts/{everything}` (JWT Bearer)

### Community — admin

```
GET|POST|PUT|DELETE|PATCH /api/community/admin/{everything}
→ :5269/api/admin/{everything}
JWT Bearer requis (Ocelot) + X-Admin-Key + X-Admin-Role (verifies par AuthorizeFilter Community)
QoS: 3 retries, 5s timeout
Rate limit: 30/min
```

## Protection en couches

| Couche                    | Mecanisme                                                        |
| ------------------------- | ---------------------------------------------------------------- |
| Gateway                   | JWT Bearer validation via Ocelot `AuthenticationOptions`         |
| Gateway                   | Rate limiting (`RateLimitOptions`)                               |
| Gateway                   | QoS (`QoSOptions`) sur routes sensibles                          |
| Community admin (en plus) | `X-Admin-Key` header validé par `AuthorizeFilter`                |
| Community admin (en plus) | `X-Admin-Role` header dans `["admin","super-admin","moderator"]` |

## Configuration

### `ocelot.json`

Configuration principale: routes, auth, rate limit, QoS, cache, Swagger endpoints.

### `appsettings*.json`

- Logging Serilog
- JWT: `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` (doivent correspondre a Identity)

### `Program.cs`

Bootstrap uniquement:

- Chargement `ocelot.json`
- Ocelot + CacheManager
- JWT Bearer auth
- CORS
- SwaggerForOcelot UI
- `await app.UseOcelot()`

## Notes

- Toutes les routes passent par Ocelot via `ocelot.json` — pas de `MapGet` custom.
- Si le Swagger aggregate echoue, verifier que les services downstream sont demarres.
- Les URLs downstream sont `localhost` — pas Docker-ready en l'etat.
