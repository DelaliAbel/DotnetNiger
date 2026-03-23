# Architecture DotnetNiger

Derniere mise a jour: 2026-03-23

## Topologie runtime

```text
Client Web/Mobile
  |
  v
Gateway (Ocelot) :5000
  |
  +--> Identity :5075
  |
  +--> Community :5269
```

## Responsabilites

### Gateway

- Routage centralise via ocelot.json
- Validation JWT (Bearer)
- Rate limiting et QoS
- Swagger agrege

### Identity

- Authentification JWT + API Key
- Gestion utilisateurs, roles, permissions
- Endpoints diagnostics (/api/v1/diagnostics/ping, /api/v1/diagnostics/health)
- Reponses de succes uniformisees via enveloppe `success/message/data/meta`
- Erreurs metier converties en `ProblemDetails` par filtre global

### Community

- Domaine communautaire: posts, comments, events, projects, resources, categories, tags, partners
- Endpoints admin (/api/v1/admin/...) proteges par filtre (X-Admin-Key, X-Admin-Role)
- Communication sortante vers Identity via IIdentityApiClient
- Reponses de succes uniformisees via enveloppe `success/message/data/meta`
- Erreurs converties globalement en `ProblemDetails` par middleware

## Securite

- Cle JWT partagee entre les trois services (Jwt:Key)
- Auth obligatoire sur les routes write et admin
- Rate limiting explicite sur routes Ocelot

## Routing Ocelot important

- Routes Community exposees via /api/...
- Route admin Community: /api/admin/community/{everything}
- Route admin Identity: /api/admin/{everything}
- Priorites de matching:
  - CommunityAdminRoute: Priority = 2
  - IdentityAdminRoute: Priority = 1

## Limitation connue

Ocelot ne permet pas nativement de definir une seule fois DownstreamScheme + DownstreamHostAndPorts pour toutes les routes.

Fin du document.
