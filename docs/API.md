# API Reference - DotnetNiger

Derniere mise a jour: 2026-03-14

Base URL Gateway (dev): <http://localhost:5000>

## Swagger

- Gateway: <http://localhost:5000/swagger>
- Identity direct: <http://localhost:5075/swagger>
- Community direct: <http://localhost:5269/swagger>

## Identity (via Gateway)

### Auth

- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/forgot-password
- POST /api/auth/reset-password
- POST /api/auth/request-email-verification
- POST /api/auth/verify-email
- POST /api/auth/refresh
- POST /api/auth/logout

### Compte

- GET /api/users
- PUT /api/users
- POST /api/users/avatar
- GET /api/users/avatar
- DELETE /api/users/avatar
- POST /api/users/change-password

### Diagnostics

- GET /api/diagnostics/ping
- GET /api/diagnostics/health

### Admin Identity

- GET /api/admin/users
- GET /api/admin/users/{id}
- PUT /api/admin/users/{id}/status
- GET /api/admin/roles
- GET /api/admin/audit/logs

## Community (via Gateway)

Routes Community exposees sous /api/... (pas /api/community/...)

### Public (GET)

- GET /api/posts
- GET /api/comments
- GET /api/events
- GET /api/projects
- GET /api/resources
- GET /api/categories
- GET /api/tags
- GET /api/partners
- GET /api/members
- GET /api/stats
- GET /api/search

### Ecriture (JWT requis)

- POST|PUT|DELETE /api/posts/{id?}
- POST|PUT|DELETE /api/comments/{id?}
- POST|PUT|DELETE /api/events/{id?}
- POST|PUT|DELETE /api/projects/{id?}
- POST|PUT|DELETE /api/resources/{id?}
- POST|PUT|DELETE /api/categories/{id?}
- POST|PUT|DELETE /api/tags/{id?}
- POST|PUT|DELETE /api/partners/{id?}
- POST|PUT|DELETE /api/members/{id?}

### Admin Community

- GET|POST|PUT|DELETE|PATCH /api/admin/community/{everything}

Requis:

- JWT valide (Gateway)
- X-Admin-Key
- X-Admin-Role (admin, super-admin, moderator)

## Endpoints importants ajoutes

### Community Updates

- PUT /api/events/{id}
- PUT /api/projects/{id}
- PUT /api/resources/{id}
- GET /api/test/identity-health

### Identity Diagnostics

- GET /api/diagnostics/ping
- GET /api/diagnostics/health

## Erreurs standards

- 400, 401, 403, 404, 429, 500

## Contrat de reponse

### Success

Les services Community et Identity renvoient des succes JSON uniformes sous la forme:

```json
{
  "success": true,
  "message": "optional human-readable message",
  "data": {},
  "meta": {}
}
```

Notes:

- `data` contient le payload principal.
- `meta` est utilise pour pagination, compteurs, ou contexte annexe.
- Les endpoints de telechargement de fichiers restent des reponses HTTP natives et ne sont pas enveloppes.

### Errors

Les erreurs applicatives sont normalisees en `application/problem+json` via `ProblemDetails`.
