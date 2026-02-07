# API_STRUCTURE.md - Project Structure

Vue d'ensemble et structure du projet DotnetNiger. Les libelles des sections et dossiers restent en anglais pour garder une convention lisible dans le code.

## 📂 Root Structure

```
DotnetNiger/
│
├── ARCHITECTURE.md              # Architecture detaillee
├── CHANGELOG.md                 # Historique des versions
├── CODE_OF_CONDUCT.md           # Code de conduite
├── CONTRIBUTING.md              # Guide de contribution
├── FAQ.md                       # Questions frequentes
├── LICENSE.md                   # Licence MIT
├── README.md                    # Documentation principale
├── SECURITY.md                  # Politique de securite
├── API_STRUCTURE.md             # Ce fichier
├── docker-compose.yml           # Orchestration services
├── run.ps1                      # Script run Windows
├── run.sh                       # Script run Linux/Mac
├── package.json                 # Outils front/format
├── package-lock.json            # Lockfile npm
├── DotnetNiger.slnx             # Solution file
├── DotnetNiger.slnLaunch.user   # Launch profile
├── .editorconfig                # Configuration IDE
├── .gitattributes               # Git attributes
├── .gitignore                   # Git ignore rules
├── .prettierignore              # Prettier ignore
├── .prettierrc                  # Prettier config
├── .github/                     # GitHub config
├── .vscode/                     # VS Code config
├── docs/                        # Public documentation
├── DotnetNiger.Gateway/         # API Gateway
├── DotnetNiger.Identity/        # Identity service
├── DotnetNiger.Community/       # Community service
├── .git/                        # Git metadata
└── .vs/                         # Visual Studio local
```

## 🧩 Services

### DotnetNiger.Gateway

```
DotnetNiger.Gateway/
├── Program.cs
├── DotnetNiger.Gateway.csproj
├── Dockerfile
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
├── DotnetNiger.Gateway.http
├── Api/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Filters/
│   └── Middleware/
├── Application/
│   ├── DTOs/
│   ├── Exceptions/
│   └── Services/
├── Infrastructure/
│   ├── Caching/
│   ├── CircuitBreaker/
│   ├── Config/
│   ├── HttpClients/
│   └── Monitoring/
├── Configuration/
│   └── yarp-routes.json
└── Properties/
    └── launchSettings.json
```

### DotnetNiger.Identity

```
DotnetNiger.Identity/
├── Program.cs
├── DotnetNiger.Identity.csproj
├── Dockerfile
├── appsettings.json
├── appsettings.Development.json
├── DotnetNiger.Identity.http
├── Api/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Filters/
│   └── Middleware/
├── Application/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Mappers/
│   ├── Services/
│   └── Validators/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
├── Infrastructure/
│   ├── Caching/
│   ├── Data/
│   ├── External/
│   ├── Migrations/
│   ├── Repositories/
│   └── Security/
└── Properties/
    └── launchSettings.json
```

### DotnetNiger.Community

```
DotnetNiger.Community/
├── Program.cs
├── DotnetNiger.Community.csproj
├── Dockerfile
├── appsettings.json
├── appsettings.Development.json
├── DotnetNiger.Community.http
├── Api/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Filters/
│   └── Middleware/
├── Application/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Mappers/
│   ├── Services/
│   └── Validators/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
├── Infrastructure/
│   ├── Caching/
│   ├── Data/
│   ├── External/
│   ├── Migrations/
│   └── Repositories/
└── Properties/
    └── launchSettings.json
```

## 📋 Configuration Files

| File              | Description                       |
| ----------------- | --------------------------------- |
| .editorconfig     | Configuration IDE unifiee         |
| .gitignore        | Fichiers a ignorer par Git         |
| .gitattributes    | Git attributes                     |
| .prettierignore   | Fichiers ignores par Prettier      |
| .prettierrc       | Configuration Prettier             |
| docker-compose.yml| Orchestration services             |
| package.json      | Scripts tooling (format)           |
| package-lock.json | Lockfile npm                       |

### Service Configuration

| Service   | Config                                                |
| --------- | ----------------------------------------------------- |
| Gateway   | appsettings.json + Configuration/yarp-routes.json     |
| Identity  | appsettings.json + migrations DB                      |
| Community | appsettings.json + migrations DB                      |

## 📚 Documentation

| File                 | Location |
| -------------------- | -------- |
| README.md            | Root     |
| ARCHITECTURE.md      | Root     |
| CHANGELOG.md         | Root     |
| CODE_OF_CONDUCT.md   | Root     |
| CONTRIBUTING.md      | Root     |
| FAQ.md               | Root     |
| LICENSE.md           | Root     |
| SECURITY.md          | Root     |
| API_STRUCTURE.md     | Root     |
| docs/00-INDEX.md     | /docs/   |
| docs/01-SETUP.md     | /docs/   |
| docs/02-QUICK-START.md | /docs/ |
| docs/03-ARCHITECTURE.md | /docs/ |
| docs/04-TECHNICAL-STACK.md | /docs/ |
| docs/05-PROJECT-STRUCTURE.md | /docs/ |
| docs/06-API.md       | /docs/   |
| docs/07-MONITORING.md | /docs/  |
| docs/08-CODE-STANDARDS.md | /docs/ |
| docs/09-TESTING.md   | /docs/   |

## 🔑 Points cles

### Clean Architecture Layers

- Api - Couche presentation (Controllers, Middleware)
- Application - Logique metier (Services, DTOs)
- Domain - Entites metier (Models, Interfaces)
- Infrastructure - Details techniques (Data, Repositories)

### Services

- Gateway - Point d'entree, routing, aggregation
- Identity - Authentification, utilisateurs
- Community - Posts, commentaires, interactions

### Patterns

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

### Developpement

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

### Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

Pour des details specifiques, consulter les fichiers de documentation appropries.
