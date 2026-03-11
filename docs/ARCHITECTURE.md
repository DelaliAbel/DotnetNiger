# Architecture DotnetNiger

Vue globale du projet DotnetNiger (.NET 8) avec API Gateway Ocelot.

> Dernière mise à jour : **2026-03-11**

## Topologie runtime

```text
Client Web / Blazor WASM / Mobile
              │
              ▼
  Gateway (DotnetNiger.Gateway, Ocelot) :5000
              │
    ┌─────────┴──────────┐
    ▼                    ▼
Identity Service      Community Service
    :5075                 :5269
```

## Vue globale de l'arborescence

```text
DotnetNiger/
├─ docs/
│  ├─ INDEX.md
│  ├─ SETUP.md
│  ├─ ARCHITECTURE.md
│  ├─ API.md
│  ├─ HEALTH_REPORT.md
│  └─ BLAZOR_WASM_INTEGRATION.md
├─ DotnetNiger.Gateway/
│  ├─ Program.cs               ← Ocelot + Polly + JWT + Swagger agrégé
│  ├─ ocelot.json              ← Routes, auth, rate limit, QoS, cache
│  ├─ appsettings.json
│  └─ appsettings.Development.json
├─ DotnetNiger.Identity/
│  ├─ Program.cs
│  ├─ Api/                     ← Controllers, Extensions, Filters, Middleware
│  ├─ Application/             ← Services, DTOs, Validators, Mappers
│  ├─ Domain/                  ← Entities, Enums, Interfaces
│  ├─ Infrastructure/          ← Repositories, Data, Caching, External
│  └─ Migrations/
├─ DotnetNiger.Community/
│  ├─ Program.cs
│  ├─ Api/                     ← Controllers (versionnés v1), Filters, Extensions
│  ├─ Application/
│  ├─ Domain/
│  ├─ Infrastructure/
│  └─ Migrations/
├─ DotnetNiger.Identity.Tests/
├─ DotnetNiger.Identity.IntegrationTests/
├─ run.sh
├─ docker-compose.yml
└─ DotnetNiger.slnx
```

## Rôles des services

### Gateway (`DotnetNiger.Gateway`)

- Point d'entrée unique pour tous les clients.
- Routing HTTP via Ocelot (`ocelot.json`).
- Validation JWT Bearer (`Ocelot.Provider.Polly` requis pour QoS).
- Rate limiting, circuit breaker/timeout (QoS Polly), cache de réponse.
- Agrégation Swagger des services downstream.

### Identity (`DotnetNiger.Identity`)

- Authentification et autorisation (dual-scheme JWT + API Key).
- Gestion utilisateurs, rôles, permissions, tokens, social links, avatars.
- Endpoints admin et diagnostics.
- Versioning v1 (`Asp.Versioning.Mvc`).

### Community (`DotnetNiger.Community`)

- Domaine communautaire : posts, comments, events, projects, resources, categories, tags, partners.
- Administration via `AdminController` (`AuthorizeFilter` : `X-Admin-Key` + `X-Admin-Role`).
- Versioning v1 (`Asp.Versioning.Mvc`) — routes `api/v{version:apiVersion}/...`.

## Sécurité par couche

### Gateway (Ocelot)

- Validation JWT Bearer sur les routes protégées.
- Rate limiting par route (30–100 req/min selon la criticité).
- QoS (circuit breaker/timeout) via `Ocelot.Provider.Polly`.
- Routes publiques sans auth (GET Community uniquement).

### Identity

- Dual-scheme : JWT Bearer + API Key (`Smart` policy scheme).
- Endpoints admin protégés (Bearer requis).

### Community

- Routes `GET` de contenu : publiques.
- Routes `POST/PUT/DELETE/PATCH` : JWT Bearer validé par le Gateway.
- Routes `/api/v1/admin/*` : `AuthorizeFilter` custom vérifiant :
  - Header `X-Admin-Key` == `Admin:ApiKey` (config ou env var `Admin__ApiKey`)
  - Header `X-Admin-Role` dans `["admin", "super-admin", "moderator"]`
- Identification utilisateur : header `X-User-Id` (injecté par Gateway) ou claims JWT.

### Clé JWT partagée

Les trois services utilisent la **même clé** `Jwt:Key` :

| Service | Utilisation |
|---------|-------------|
| Identity | Émet et signe les tokens |
| Gateway  | Valide les tokens avant routage |
| Community | Peut valider les tokens localement si nécessaire |

## Modèle de données Community

Entités principales (EF Core SQLite, `Data Source=../DotnetNiger.db`) :

| Entité | Champs clés |
|--------|-------------|
| `Post` | `IsPublished`, `CategoryId`, `Tags`, `AuthorId` |
| `Comment` | `IsApproved` (auto `true` à la création) |
| `Event` | `IsPublished`, `Location`, `StartDate`, `EndDate` |
| `Project` | `IsActive`, `IsFeatured`, `OwnerId` |
| `Resource` | `IsApproved`, `Type` |
| `Category` | Catégories posts |
| `Tag` | Tags posts |
| `Partner` | Partenaires |
| `Member` / `MemberSkill` | **DÉSACTIVÉ** (controller et service commentés) |

## Observabilité

- Logging via Serilog (fichiers rotatifs dans `logs/` de chaque service).
- Correlation ID propagé via header `X-Request-ID` au niveau Gateway.
- Endpoint `/health` exposé via Gateway (`GET /health`).
- Swagger UI sur chaque service + agrégé sur le Gateway.

## État des fonctionnalités

| Fonctionnalité | Statut |
|----------------|--------|
| Gateway Ocelot + Polly (QoS) | ✅ Complet |
| Identity auth JWT + API Key | ✅ Complet |
| Identity versioning v1 | ✅ Complet |
| Community versioning v1 | ✅ Complet |
| Community CRUD public | ✅ Complet |
| Community admin | ✅ Complet |
| Clé JWT unifiée (3 services) | ✅ Complet |
| Identity unit tests (7/7) | ✅ Passants |
| Identity integration tests | ⚠️ Partiellement validés |
| Member feature (Community) | ❌ Désactivé |
| Community JWT local validation | ❌ Dépend du Gateway uniquement |

## Notes d'évolution

- La feature `Member` est entièrement commentée dans Community — à activer quand le service sera prêt.
- En production, `Admin__ApiKey` doit être défini via variable d'environnement (jamais committé).
- Community ne valide pas le JWT localement : il fait confiance au header `X-User-Id` injecté par le Gateway.

## Topologie runtime

```text
Client Web/Mobile
       |
       v
Gateway (DotnetNiger.Gateway, Ocelot) :5000
       |
       +--> Identity Service  :5075
       |
       +--> Community Service :5269
```

## Vue globale de l'arborescence

```text
DotnetNiger/
├─ docs/
├─ DotnetNiger.Gateway/
│  ├─ Program.cs
│  ├─ ocelot.json
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Dockerfile
│  ├─ DotnetNiger.Gateway.csproj
│  ├─ README.md
│  ├─ Properties/
│  ├─ logs/
│  ├─ bin/
│  └─ obj/
├─ DotnetNiger.Identity/
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Dockerfile
│  ├─ DotnetNiger.Identity.csproj
│  ├─ Api/
│  ├─ Application/
│  ├─ Domain/
│  ├─ Infrastructure/
│  ├─ Migrations/
│  ├─ Properties/
│  ├─ uploads/
│  ├─ logs/
│  ├─ bin/
│  └─ obj/
├─ DotnetNiger.Community/
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Dockerfile
│  ├─ DotnetNiger.Community.csproj
│  ├─ Api/
│  ├─ Application/
│  ├─ Domain/
│  ├─ Infrastructure/
│  ├─ Migrations/
│  ├─ Properties/
│  ├─ bin/
│  └─ obj/
├─ DotnetNiger.Identity.Tests/
│  ├─ *.cs
│  ├─ bin/
│  └─ obj/
├─ DotnetNiger.Identity.IntegrationTests/
│  ├─ *.cs
│  ├─ bin/
│  └─ obj/
├─ run.sh
├─ docker-compose.yml
└─ DotnetNiger.slnx
```

## Roles des services

### Gateway (`DotnetNiger.Gateway`)

- Point d'entree unique pour les clients.
- Routage HTTP via Ocelot (`ocelot.json`).
- Validation JWT Bearer par route.
- Rate limiting, QoS, cache de reponse par route.
- Aggregation Swagger des services downstream.

### Identity (`DotnetNiger.Identity`)

- Authentification et autorisation.
- Gestion utilisateurs, roles, permissions, tokens.
- Endpoints admin et diagnostics.

### Community (`DotnetNiger.Community`)

- Domaine communautaire: posts, comments, events, projects, resources, categories, tags, partners, team.
- Administration via `AdminController` (protege par `AuthorizeFilter` + `X-Admin-Key` + `X-Admin-Role`).
- Expose les endpoints metier via son API REST.

## Securite par couche

### Gateway (Ocelot)

- Validation JWT Bearer sur les routes protegees.
- Rate limiting par route (30-100 req/min selon la criticite).
- QoS (retries/timeout) sur certaines routes admin.
- Routes publiques sans auth (GET uniquement pour Community).

### Identity

- Dual-scheme auth: JWT Bearer + API Key (`Smart` policy scheme).
- Gestion utilisateurs, roles, permissions, tokens, social links.
- Endpoints admin proteges (Bearer requis).

### Community

- Routes `GET` de contenu: publiques (pas d'auth requise).
- Routes `POST/PUT/DELETE/PATCH` de contenu: JWT Bearer via Gateway.
- Routes `/api/admin/*`: `AuthorizeFilter` custom verifiant:
  - `X-Admin-Key` header == `Admin:ApiKey` en config
  - `X-Admin-Role` header dans `["admin","super-admin","moderator"]`
- Auth utilisateur dans les controllers: header `X-User-Id` (injecte par le Gateway) ou claims JWT.

## Modele de donnees Community

Les entites principales (EF Core SQLite, `Data Source=../DotnetNiger.db`):

- `Post` — articles de blog (IsPublished, CategoryId, Tags, AuthorId)
- `Comment` — commentaires sur posts (IsApproved=true auto a la creation)
- `Event` — evenements communautaires (IsPublished, Location, StartDate/EndDate)
- `Project` — projets membres (IsActive, IsFeatured, OwnerId)
- `Resource` — ressources partageables (IsApproved, Type)
- `Category` — categories posts
- `Tag` — tags posts
- `Partner` — partenaires
- `Member` / `MemberSkill` — profils membres (DESACTIVE — controller et service commentes)

## Configuration et conventions

- Bootstrap de chaque service dans son `Program.cs`.
- Routage gateway centralise dans `DotnetNiger.Gateway/ocelot.json`.
- Configuration environnement via `appsettings*.json` + variables d'environnement.
- Script principal local: `run.sh`.
- Seed de donnees de dev automatique au demarrage Community (categories, tags, posts, events...).

## Observabilite

- Logging applicatif via Serilog (fichiers rotatifs dans `logs/`).
- Correlation ID au niveau gateway.
- Endpoint `/health` expose via gateway (`GET /health`).
- Swagger UI disponible sur chaque service et aggrege sur le gateway.

## Etat des fonctionnalites

| Fonctionnalite                                             | Statut                                     |
| ---------------------------------------------------------- | ------------------------------------------ |
| Gateway Ocelot                                             | ✅ Complet                                 |
| Identity auth JWT + API Key                                | ✅ Complet                                 |
| Community CRUD public                                      | ✅ Complet                                 |
| Community admin (posts/events/projects/resources/comments) | ✅ Complet                                 |
| Identity unit tests (7/7)                                  | ✅ Passants                                |
| Identity integration tests                                 | ⚠️ Infra corrigee, non re-valides          |
| Member feature (Community)                                 | ❌ Desactive (non implemente)              |
| Community write endpoint Authorize                         | ❌ Manquant (depend du Gateway uniquement) |
| Admin:ApiKey production                                    | ❌ Vide dans appsettings.json de prod      |

## Notes d'evolution

- L'implementation active du gateway est Ocelot.
- La feature `Member` (profils membres) est entierement commentee dans Community — a activer quand le service sera pret.
- Community ne valide pas le JWT localement: il fait confiance au header `X-User-Id` injecte par le Gateway. A securiser si Community est expose directement.
- `Admin:ApiKey` doit etre configure via env var (`Admin__ApiKey`) en production — ne jamais committer de vraie cle.
