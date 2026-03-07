# Architecture DotnetNiger

Vue globale du projet DotnetNiger (.NET 8) avec API Gateway Ocelot.

## Derniere mise a jour

- 2026-03-07

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

- Domaine communautaire: posts, comments, events, projects, resources.
- Expose les endpoints metier via son API.

## Configuration et conventions

- Bootstrap de chaque service dans son `Program.cs`.
- Routage gateway centralise dans `DotnetNiger.Gateway/ocelot.json`.
- Configuration environnement via `appsettings*.json` + variables d'environnement.
- Script principal local: `run.sh` (build, clean, init-db, run/watch, stop, status).

## Observabilite

- Logging applicatif via Serilog.
- Correlation ID au niveau gateway.
- Endpoint de sante expose via le gateway et les services.

## Notes d'evolution

- L'implementation active du gateway est Ocelot.
- Les anciens scripts `init-shared-db.sh` et `start-all-services.sh` ont ete fusionnes dans `run.sh`.
