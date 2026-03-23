# DotnetNiger

Plateforme communautaire microservices .NET 8 avec API Gateway Ocelot.

> Dernière mise à jour : **2026-03-23**

## Services

| Service                             | Port   | Description                                                         |
| ----------------------------------- | ------ | ------------------------------------------------------------------- |
| Gateway (`DotnetNiger.Gateway`)     | `5000` | Point d'entrée unique, routing Ocelot, auth JWT, rate limiting, QoS |
| Identity (`DotnetNiger.Identity`)   | `5075` | Authentification, utilisateurs, rôles, permissions, administration  |
| Community (`DotnetNiger.Community`) | `5269` | Posts, commentaires, events, projets, ressources, catégories, tags  |

## Démarrage rapide

```bash
git clone https://github.com/DelaliAbel/DotnetNiger.git
cd DotnetNiger
dotnet restore
```

Lancer dans cet ordre: Identity, puis Community, puis Gateway.

```bash
# Terminal 1
cd DotnetNiger.Identity
dotnet run

# Terminal 2
cd DotnetNiger.Community
dotnet run

# Terminal 3
cd DotnetNiger.Gateway
dotnet run
```

## URLs utiles

- `http://localhost:5000/swagger` (Swagger Gateway)
- `http://localhost:5000/health` (health Gateway)
- `http://localhost:5075/swagger` (Swagger Identity)
- `http://localhost:5269/swagger` (Swagger Community)

## Points clés

- Versioning API activé: `api/v1/...` pour Identity et Community.
- Communication Community vers Identity disponible via client HTTP typé (`IIdentityApiClient`).
- Endpoints Diagnostics Identity exposés: `GET /api/v1/diagnostics/ping` et `GET /api/v1/diagnostics/health`.
- Endpoints update Community réactivés pour Events, Projects et Resources.
- Route admin Community priorisée dans Ocelot pour éviter le conflit avec la route admin Identity.

## Configuration essentielle

| Variable               | Usage                                               |
| ---------------------- | --------------------------------------------------- |
| `Jwt__Key`             | Clé JWT partagée entre Gateway, Identity, Community |
| `Jwt__Issuer`          | Émetteur JWT                                        |
| `Jwt__Audience`        | Audience JWT                                        |
| `Admin__ApiKey`        | Clé admin Community (`X-Admin-Key`)                 |
| `IdentityApi__BaseUrl` | URL de base Identity utilisée par Community         |

> En production, utiliser variables d'environnement ou user-secrets pour les secrets.

## Documentation

- [docs/INDEX.md](docs/INDEX.md)
- [docs/SETUP.md](docs/SETUP.md)
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- [docs/API.md](docs/API.md)
- [DotnetNiger.Gateway/README.md](DotnetNiger.Gateway/README.md)
- [DotnetNiger.Identity/README.md](DotnetNiger.Identity/README.md)
