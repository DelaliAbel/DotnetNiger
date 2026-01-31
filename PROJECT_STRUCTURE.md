# PROJECT_STRUCTURE.md - Vue complète du projet

Structure complète et détaillée de DotnetNiger après setup initial.

## 📂 Structure racine

```
DotnetNiger/
│
├── 📄 README.md                    # Documentation principale
├── 📄 LICENSE.md                   # Licence MIT
├── 📄 CODE_OF_CONDUCT.md           # Code de conduite
├── 📄 CONTRIBUTING.md              # Guide de contribution
├── 📄 SECURITY.md                  # Politique de sécurité
├── 📄 ARCHITECTURE.md              # Architecture détaillée
├── 📄 API.md                       # Documentation API
├── 📄 SETUP.md                     # Guide de setup
├── 📄 DEPLOYMENT.md                # Guide de déploiement
├── 📄 TESTING.md                   # Guide de test
├── 📄 CHANGELOG.md                 # Historique des versions
├── 📄 FAQ.md                       # Questions fréquentes
├── 📄 VERSION                      # Version actuelle (1.0.0)
├── 📄 PROJECT_STRUCTURE.md         # Ce fichier - Structure complète
├── 📄 .editorconfig                # Configuration IDE
├── 📄 .gitignore                   # Git ignore rules
├── 📄 .gitattributes               # Git attributes
├── 📄 docker-compose.yml           # Orchestration services
├── 📄 DotnetNiger.slnx             # Solution file
│
├── 📁 DotnetNiger.Gateway/
│   ├── 📄 Program.cs               # Point d'entrée
│   ├── 📄 DotnetNiger.Gateway.csproj
│   ├── 📄 Dockerfile               # Multi-stage build
│   ├── 📄 .dockerignore
│   ├── 📄 appsettings.json
│   ├── 📄 appsettings.Development.json
│   ├── 📄 appsettings.Production.json
│   ├── 📄 DotnetNiger.Gateway.http # Requêtes HTTP
│   ├── 📄 SwaggerAggregatorController.cs
│   │
│   ├── 📁 Api/
│   │   ├── 📁 Middleware/
│   │   │   ├── RateLimitingMiddleware.cs
│   │   │   ├── AuthenticationMiddleware.cs
│   │   │   ├── JwtInjectionMiddleware.cs
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── CorsMiddleware.cs
│   │   │   └── RequestTransformMiddleware.cs
│   │   ├── 📁 Filters/
│   │   │   ├── ExceptionFilter.cs
│   │   │   └── ValidationFilter.cs
│   │   ├── 📁 Extensions/
│   │   │   ├── ServiceExtensions.cs
│   │   │   └── MiddlewareExtensions.cs
│   │   └── 📁 Controllers/
│   │       ├── HealthCheckController.cs
│   │       └── SwaggerAggregatorController.cs
│   │
│   ├── 📁 Application/
│   │   ├── 📁 Services/
│   │   │   ├── IRouteService.cs
│   │   │   ├── RouteService.cs
│   │   │   ├── IRateLimitService.cs
│   │   │   ├── RateLimitService.cs
│   │   │   ├── IAuthenticationService.cs
│   │   │   ├── AuthenticationService.cs
│   │   │   ├── IRequestForwardingService.cs
│   │   │   ├── RequestForwardingService.cs
│   │   │   ├── ICachingService.cs
│   │   │   ├── CachingService.cs
│   │   │   ├── IMetricsService.cs
│   │   │   └── MetricsService.cs
│   │   ├── 📁 DTOs/
│   │   │   ├── 📁 Requests/
│   │   │   │   └── ForwardRequest.cs
│   │   │   └── 📁 Responses/
│   │   │       ├── GatewayResponse.cs
│   │   │       ├── ErrorResponse.cs
│   │   │       └── MetricsDto.cs
│   │   └── 📁 Exceptions/
│   │       ├── GatewayException.cs
│   │       ├── RateLimitExceededException.cs
│   │       ├── ServiceUnavailableException.cs
│   │       ├── UnauthorizedException.cs
│   │       └── RouteNotFoundException.cs
│   │
│   ├── 📁 Infrastructure/
│   │   ├── 📁 Config/
│   │   │   ├── GatewayConfig.cs
│   │   │   ├── RateLimitConfig.cs
│   │   │   ├── ServiceRegistry.cs
│   │   │   └── RouteConfiguration.cs
│   │   ├── 📁 HttpClients/
│   │   │   ├── ICommunityApiClient.cs
│   │   │   ├── CommunityApiClient.cs
│   │   │   ├── IIdentityApiClient.cs
│   │   │   ├── IdentityApiClient.cs
│   │   │   └── ApiClientBase.cs
│   │   ├── 📁 Caching/
│   │   │   ├── ICacheProvider.cs
│   │   │   └── RedisCacheProvider.cs
│   │   ├── 📁 Monitoring/
│   │   │   ├── IHealthCheckService.cs
│   │   │   ├── HealthCheckService.cs
│   │   │   ├── IMetricsCollector.cs
│   │   │   └── PrometheusMetricsCollector.cs
│   │   └── 📁 CircuitBreaker/
│   │       ├── ICircuitBreaker.cs
│   │       └── PollyCircuitBreaker.cs
│   │
│   ├── 📁 Configuration/
│   │   └── yarp-routes.json
│   │
│   └── 📁 Properties/
│       └── launchSettings.json
│
├── 📁 DotnetNiger.Identity/
│   ├── 📄 Program.cs
│   ├── 📄 DotnetNiger.Identity.csproj
│   ├── 📄 Dockerfile
│   ├── 📄 .dockerignore
│   ├── 📄 appsettings.json
│   ├── 📄 appsettings.Development.json
│   ├── 📄 DotnetNiger.Identity.http
│   │
│   ├── 📁 Api/
│   │   ├── 📁 Controllers/
│   │   ├── 📁 Middleware/
│   │   ├── 📁 Filters/
│   │   └── 📁 Extensions/
│   │
│   ├── 📁 Application/
│   │   ├── 📁 Services/
│   │   ├── 📁 DTOs/
│   │   ├── 📁 Mappers/
│   │   ├── 📁 Validators/
│   │   └── 📁 Exceptions/
│   │
│   ├── 📁 Domain/
│   │   ├── 📁 Entities/
│   │   ├── 📁 Enums/
│   │   └── 📁 Interfaces/
│   │
│   ├── 📁 Infrastructure/
│   │   ├── 📁 Data/
│   │   ├── 📁 Repositories/
│   │   ├── 📁 Security/
│   │   ├── 📁 Caching/
│   │   ├── 📁 External/
│   │   └── 📁 Migrations/
│   │
│   └── 📁 Properties/
│       └── launchSettings.json
│
├── 📁 DotnetNiger.Community/
│   ├── 📄 Program.cs
│   ├── 📄 DotnetNiger.Community.csproj
│   ├── 📄 Dockerfile
│   ├── 📄 .dockerignore
│   ├── 📄 appsettings.json
│   ├── 📄 appsettings.Development.json
│   ├── 📄 DotnetNiger.Community.http
│   │
│   ├── 📁 Api/
│   │   ├── 📁 Controllers/
│   │   ├── 📁 Middleware/
│   │   ├── 📁 Filters/
│   │   └── 📁 Extensions/
│   │
│   ├── 📁 Application/
│   │   ├── 📁 Services/
│   │   ├── 📁 DTOs/
│   │   ├── 📁 Mappers/
│   │   ├── 📁 Validators/
│   │   └── 📁 Exceptions/
│   │
│   ├── 📁 Domain/
│   │   ├── 📁 Entities/
│   │   ├── 📁 Enums/
│   │   └── 📁 Interfaces/
│   │
│   ├── 📁 Infrastructure/
│   │   ├── 📁 Data/
│   │   ├── 📁 Repositories/
│   │   ├── 📁 Caching/
│   │   ├── 📁 External/
│   │   └── 📁 Migrations/
│   │
│   └── 📁 Properties/
│       └── launchSettings.json
│
├── 📁 .github/
│   ├── 📄 CODEOWNERS                # Code owners
│   ├── 📄 README.md                 # GitHub repository info
│   ├── 📄 PULL_REQUEST_TEMPLATE.md  # PR template
│   ├── 📁 ISSUE_TEMPLATE/
│   │   ├── 📄 bug_report.md
│   │   ├── 📄 feature_request.md
│   │   └── 📄 config.yml
│   └── 📁 workflows/                # GitHub Actions CI/CD
│       ├── 📄 ci.yml                # Continuous Integration
│       ├── 📄 tests.yml             # Automated tests
│       ├── 📄 docker.yml            # Docker build & push
│       ├── 📄 deploy.yml            # Deployment workflow
│       └── 📄 sonar.yml             # SonarQube analysis
│
├── 📁 docs/                        # PUBLIC documentation (GitHub)
│   ├── 📄 00-INDEX.md              # Documentation index
│   ├── 📄 01-SETUP.md              # Installation guide
│   ├── 📄 02-QUICK-START.md        # 5-minute quickstart
│   ├── 📄 03-ARCHITECTURE.md       # Architecture overview
│   ├── 📄 04-TECHNICAL-STACK.md    # └── 📁 .vscode/                     # VS Code configuration
    ├── 📄 extensions.json
    ├── 📄 launch.json
    └── 📄 tasks.json
```

## 📋 Fichiers de configuration

### Configuration du projet

| Fichier         | Description                   |
| --------------- | ----------------------------- |
| `.editorconfig` | Configuration IDE unifiée     |
| `.gitignore`    | Fichiers à ignorer par Git    |
| `.dockerignore` | Fichiers à ignorer par Docker |
| `VERSION`       | Version du projet             |

### Configuration des services

| Service   | Config                                                |
| --------- | ----------------------------------------------------- |
| Gateway   | `appsettings.json` + `Configuration/yarp-routes.json` |
| Identity  | `appsettings.json` + Migration DB                     |
| Community | `appsettings.json` + Migration DB                     |

## 📚 Documentation

| Fichier                    | Objectif                               | Location                   |
| -------------------------- | -------------------------------------- | -------------------------- |
| README.md                  | Vue d'ensemble du projet               | Root                       |
| SETUP.md                   | Guide d'installation                   | Root                       |
| ARCHITECTURE.md            | Architecture détaillée                 | Root                       |
| API.md                     | Documentation API complète             | Root                       |
| CONTRIBUTING.md            | Guide pour contributeurs               | Root                       |
| DEPLOYMENT.md              | Guide de déploiement                   | Root                       |
| TESTING.md                 | Guide des tests                        | Root                       |
| SECURITY.md                | Politique de sécurité                  | Root                       |
| CHANGELOG.md               | Historique des versions                | Root                       |
| FAQ.md                     | Questions fréquentes                   | Root                       |
| DOCUMENTATION-STRUCTURE.md | Classification documentation           | Root                       |
| PROJECT_STRUCTURE.md       | Structure complète (ce fichier)        | Root                       |
| **Dossier `/docs/`**       | **PUBLIC documentation (8 files)**     | **GitHub visible**         |
| 00-INDEX.md                | Documentation index                    | /docs/                     |
| 01-SETUP.md                | Installation guide                     | /docs/                     |
| 02-QUICK-START.md          | 5-minute quickstart                    | /docs/                     |
| 03-ARCHITECTURE.md         | Architecture overview                  | /docs/                     |
| 04-TECHNICAL-STACK.md      | Tech stack (.NET 8.0 LTS)              | /docs/                     |
| 05-PROJECT-STRUCTURE.md    | Project structure detail               | /docs/                     |
| 06-API.md                  | API endpoints                          | /docs/                     |
| 08-DEPLOYMENT.md           | Deployment guide                       | /docs/                     |
| **Dossier `/devteam/`**    | **PRIVATE documentation (23 files)**   | **Gitignored - Team only** |
| 00-DEVTEAM-INDEX.md        | Team documentation index               | /devteam/                  |
| ONBOARDING.md              | Developer onboarding                   | /devteam/                  |
| CODE-STANDARDS.md          | Coding standards                       | /devteam/                  |
| TESTING-GUIDE.md           | Testing strategies                     | /devteam/                  |
| Endpoints (6 files)        | Identity, Community, Gateway endpoints | /devteam/                  |
| MODELS-STRUCTURE.md        | Database models                        | /devteam/                  |
| MONITORING.md              | Monitoring & logging                   | /devteam/                  |
| PERFORMANCE-TUNING.md      | Performance optimization               | /devteam/                  |
| SECRETS-MANAGEMENT.md      | Secrets management                     | /devteam/                  |
| And 10+ more files         | Supporting documentation               | /devteam/                  |

## 🔑 Points clés

### Structure Clean Architecture

- **Api** - Couche présentation (Controllers, Middleware)
- **Application** - Logique métier (Services, DTOs)
- **Domain** - Entités métier (Models, Interfaces)
- **Infrastructure** - Détails techniques (Data, Repositories)

### Services

- **Gateway** - Point d'entrée, routing, aggregation
- **Identity** - Authentification, utilisateurs
- **Community** - Posts, commentaires, interactions

### Patterns utilisés

- Repository Pattern
- Dependency Injection
- Middleware Pipeline
- Circuit Breaker
- Caching Strategy

### Technologies

- .NET 8.0 LTS
- YARP (Reverse Proxy)
- SQL Server 2022
- Redis
- Docker
- Prometheus

## 🚀 Commandes utiles

### Setup initial

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
dotnet restore
docker-compose up
```

### Développement

```bash
dotnet build
dotnet run
dotnet test
```

### Docker

```bash
docker-compose up -d
docker-compose ps
docker-compose logs -f gateway
```

### Gestion des migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

Pour des détails spécifiques, consulter les fichiers de documentation appropriés.
