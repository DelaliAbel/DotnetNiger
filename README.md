# DotnetNiger

Plateforme communautaire microservices construite avec .NET 8.

> Dernière mise à jour : **2026-03-11**

## Services

| Service                             | Port   | Description                                             |
| ----------------------------------- | ------ | ------------------------------------------------------- |
| Gateway (`DotnetNiger.Gateway`)     | `5000` | Ocelot — point d'entrée unique, JWT, rate limiting, QoS |
| Identity (`DotnetNiger.Identity`)   | `5075` | Auth JWT + API Key, users, rôles, permissions, admin    |
| Community (`DotnetNiger.Community`) | `5269` | Posts, events, projets, ressources, catégories, admin   |

## Démarrage rapide

```bash
git clone https://github.com/DelaliAbel/DotnetNiger.git
cd DotnetNiger
dotnet restore
```

Lancer les 3 services **dans cet ordre** — Identity → Community → Gateway :

```bash
# Terminal 1
cd DotnetNiger.Identity  && dotnet run

# Terminal 2
cd DotnetNiger.Community && dotnet run

# Terminal 3
cd DotnetNiger.Gateway   && dotnet run
```

## URLs utiles (dev)

| URL                             | Description            |
| ------------------------------- | ---------------------- |
| `http://localhost:5000/swagger` | Swagger agrégé Gateway |
| `http://localhost:5000/health`  | Health check Gateway   |
| `http://localhost:5075/swagger` | Swagger Identity       |
| `http://localhost:5269/swagger` | Swagger Community      |

## Fonctionnalités

- API Gateway Ocelot (routing centralisé, JWT, rate limiting, QoS Polly, cache)
- Authentification JWT + API Key dual-scheme (Identity)
- Versioning API `v1` sur Identity et Community (`Asp.Versioning.Mvc`)
- Gestion utilisateurs, rôles, permissions, social links, avatars
- Admin Identity (users, api-keys, audit logs)
- Community : posts, events, projets, ressources, catégories, tags, partenaires
- Admin Community : tableau de bord, modération ressources, publication posts/events, projets en avant
- Logs Serilog sur tous les services
- SQLite partagé (aucun Docker requis en dev)

## Tests

```bash
dotnet test DotnetNiger.Identity.Tests/          # 7/7 unitaires
dotnet test DotnetNiger.Identity.IntegrationTests/
dotnet test                                       # tous
```

## Documentation

| Fichier                                                            | Contenu                                   |
| ------------------------------------------------------------------ | ----------------------------------------- |
| [docs/INDEX.md](docs/INDEX.md)                                     | Index général                             |
| [docs/SETUP.md](docs/SETUP.md)                                     | Guide d'installation et dépannage         |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)                       | Architecture, sécurité, modèle de données |
| [docs/API.md](docs/API.md)                                         | Référence complète des endpoints          |
| [docs/HEALTH_REPORT.md](docs/HEALTH_REPORT.md)                     | Santé technique                           |
| [docs/BLAZOR_WASM_INTEGRATION.md](docs/BLAZOR_WASM_INTEGRATION.md) | Guide interconnexion Blazor WASM          |

## Configuration essentielle

| Variable        | Usage                                      | Services concernés             |
| --------------- | ------------------------------------------ | ------------------------------ |
| `Jwt__Key`      | Clé JWT partagée (≥ 32 chars)              | Gateway + Identity + Community |
| `Jwt__Issuer`   | Émetteur JWT                               | Les 3                          |
| `Jwt__Audience` | Audience JWT                               | Les 3                          |
| `Admin__ApiKey` | Clé admin Community (header `X-Admin-Key`) | Community                      |

> **Production** : ne jamais committer de vraies clés. Utiliser `dotnet user-secrets` ou variables d'environnement.

## Notes

- Le routing gateway est configuré dans `DotnetNiger.Gateway/ocelot.json`.
- Les routes Community passent toutes par `api/v1/...` depuis la migration versioning.
- `Ocelot.Provider.Polly` est requis pour les `QoSOptions` (circuit breaker, timeout).

## Services

| Service                             | Port   | Description                               |
| ----------------------------------- | ------ | ----------------------------------------- |
| Gateway (`DotnetNiger.Gateway`)     | `5000` | Ocelot — point d'entree unique            |
| Identity (`DotnetNiger.Identity`)   | `5075` | Auth JWT + API Key, users, admin          |
| Community (`DotnetNiger.Community`) | `5269` | Posts, events, projects, resources, admin |

## Demarrage rapide

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
dotnet restore
```

Lancer les 3 services (ordre conseille: Identity → Community → Gateway):

```bash
./run.sh      # Linux/Mac
# ou manuellement dans 3 terminaux:
cd DotnetNiger.Identity && dotnet run
cd DotnetNiger.Community && dotnet run
cd DotnetNiger.Gateway && dotnet run
```

## URLs utiles

- Gateway Swagger aggregate: `http://localhost:5000/swagger`
- Gateway health: `http://localhost:5000/health`
- Identity Swagger: `http://localhost:5075/swagger`
- Community Swagger: `http://localhost:5269/swagger`

## Fonctionnalites

- API Gateway Ocelot (routing centralise, JWT, rate limiting, QoS, cache)
- Authentification JWT + API Key dual-scheme (Identity)
- Gestion utilisateurs, roles, permissions, social links, avatars
- Admin Identity (users, api-keys, audit logs)
- Community: posts, events, projects, resources, categories, tags, partenaires
- Admin Community: tableau de bord, moderation ressources, publication posts/events, projets en avant
- Logs Serilog sur tous les services
- SQLite partage (pas de Docker requis en dev)

## Tests

```bash
dotnet test DotnetNiger.Identity.Tests/       # 7/7 unitaires
dotnet test DotnetNiger.Identity.IntegrationTests/
dotnet test                                    # tous
```

## Documentation

- [docs/INDEX.md](docs/INDEX.md) — Index
- [docs/SETUP.md](docs/SETUP.md) — Guide d'installation
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — Architecture
- [docs/API.md](docs/API.md) — Reference API complete
- [docs/HEALTH_REPORT.md](docs/HEALTH_REPORT.md) — Sante technique et prochaines etapes

## Configuration essentielle

| Variable        | Usage                                     |
| --------------- | ----------------------------------------- |
| `Jwt__Key`      | Cle JWT (meme valeur Gateway et Identity) |
| `Jwt__Issuer`   | Emetteur JWT                              |
| `Jwt__Audience` | Audience JWT                              |
| `Admin__ApiKey` | Cle admin Community (obligatoire en prod) |

> Ne jamais committer de vraies cles. Utiliser des variables d'environnement ou `dotnet user-secrets` en production.

## Notes

- Le routing gateway est configure dans `DotnetNiger.Gateway/ocelot.json`.
- Les routes protegees utilisent `AuthenticationProviderKey: "Bearer"`.
- Les routes admin Community necessitent en plus les headers `X-Admin-Key` et `X-Admin-Role`.
- La feature Member est desactivee (code presente mais commente).
