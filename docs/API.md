# API Reference

## Base URLs

| Environment | Gateway | Identity | Community |
|-------------|---------|----------|-----------|
| Development | `http://localhost:5000` | `http://localhost:5075` | `http://localhost:5269` |
| Docker | `http://localhost:5000` | `http://localhost:8081` | `http://localhost:8082` |

In production, **only the Gateway** port (`5000`) should be exposed. Downstream services are internal.

## Swagger UIs

| URL | Description |
|-----|-------------|
| `http://localhost:5000/swagger` | Aggregated (all services) |
| `http://localhost:5075/swagger` | Identity only (direct) |
| `http://localhost:5269/swagger` | Community only (direct) |

## Gateway Endpoints

### Health, Metrics & Service Registry

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Gateway liveness |
| GET | `/health/ready` | Readiness (all downstreams must respond) |
| GET | `/health/downstream` | Detailed downstream health (static + dynamic services) |
| GET | `/health/services` | Registered services config (static + dynamic) |
| POST | `/api/service-registry/register` | Dynamic service registration |
| GET | `/metrics/latency` | Endpoint latency (P50/P95/P99) |

### Routing Map

The Gateway maps simplified upstream paths to versioned downstream paths:

#### Identity Routes (→ `localhost:5075`)

| Upstream (Gateway) | Downstream (Identity) | Auth | Notes |
|--------------------|----------------------|------|-------|
| `POST /api/super-admin` | `POST /bootstrap/super-admin` | — | Bootstrap first super admin |
| `/api/auth/{everything}` | `/api/v1/auth/{everything}` | Mixed | Login public, logout protected |
| `/api/me/{everything}` | `/api/v1/me/{everything}` | Bearer | User profile |
| `/api/tokens/{everything}` | `/api/v1/tokens/{everything}` | Bearer | API tokens |
| `/api/social-links/{everything}` | `/api/v1/social-links/{everything}` | Bearer | Social links |
| `/api/roles/{everything}` | `/api/v1/roles/{everything}` | Bearer | Role management |
| `/api/permissions/{everything}` | `/api/v1/permissions/{everything}` | Bearer | Permission management |
| `/api/api-keys/{everything}` | `/api/v1/api-keys/{everything}` | Bearer | API keys |
| `/api/identity/admin/{everything}` | `/api/v1/admin/{everything}` | Bearer+Admin | Admin (requires `is_super_admin` claim) |
| `GET /api/diagnostics/{everything}` | `GET /api/v1/diagnostics/{everything}` | — | Health, ping |

#### Community Routes (→ `localhost:5269`)

| Upstream (Gateway) | Downstream (Community) | Auth | Notes |
|--------------------|----------------------|------|-------|
| `GET /api/test/{everything}` | `GET /api/v1/test/{everything}` | — | Diagnostics |
| `/api/posts/{everything}` | `/api/v1/posts/{everything}` | Mixed | GET public, write requires Bearer |
| `/api/comments/{everything}` | `/api/v1/comments/{everything}` | Mixed | GET public, write requires Bearer |
| `/api/events/{everything}` | `/api/v1/events/{everything}` | Mixed | GET public, write requires Bearer |
| `/api/newsletters/**` | `/api/v1/newsletters/**` | Mixed | Subscribe public, manage requires Bearer |
| `/api/projects/{everything}` | `/api/v1/projects/{everything}` | — | Public |
| `/api/resources/{everything}` | `/api/v1/resources/{everything}` | — | Public |
| `/api/categories/{everything}` | `/api/v1/categories/{everything}` | — | Public |
| `/api/tags/{everything}` | `/api/v1/tags/{everything}` | — | Public |
| `/api/partners/{everything}` | `/api/v1/partners/{everything}` | — | Public |
| `/api/members/{everything}` | `/api/v1/members/{everything}` | — | Public |
| `GET /api/stats/{everything}` | `GET /api/v1/stats/{everything}` | — | Public |
| `GET /api/search/{everything}` | `GET /api/v1/search/{everything}` | — | Public |
| `/api/community/admin/{everything}` | `/api/v1/admin/{everything}` | Bearer+Admin | Requires `is_admin` claim |

## Authentication

### Obtaining a Token

```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&username=user@example.com&password=MyPass@123&scope=openid+profile+email+roles+offline_access
```

### Using a Token

```http
GET /api/me/profile
Authorization: Bearer eyJhbG...
```

## Response Format

All Community endpoints return:

```json
{
  "success": true,
  "data": { ... }
}
```

Paginated responses:

```json
{
  "success": true,
  "data": {
    "items": [],
    "totalCount": 0,
    "page": 1,
    "pageSize": 10,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Identity uses OpenIddict standards for `/connect/token` and custom DTOs for CRUD endpoints.

## Headers

| Header | Description |
|--------|-------------|
| `Authorization: Bearer <token>` | JWT authentication |
| `X-Request-ID` | Request tracing (propagated by Gateway) |
| `ClientId` / `Oc-Client` | Rate limiting client identification |
| `X-Tenant-Id` | Tenant context for multi-tenant requests |
