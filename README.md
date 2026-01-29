# DotnetNiger - API Gateway & Microservices Platform

Une plateforme d'architecture microservices moderne basée sur .NET 8 LTS avec Gateway API centralisé, gestion d'identité et services communautaires.

## 🎯 Vue d'ensemble

DotnetNiger est une solution complète de microservices qui démontre les meilleures pratiques pour :
- Gateway API avec YARP (Yet Another Reverse Proxy)
- Gestion centralisée des identités
- Services modulaires et scalables
- Containerisation avec Docker
- Monitoring et métriques
- Rate limiting et circuit breaker
- Documentation automatisée Swagger

## 🏗️ Architecture

```
DotnetNiger/
├── DotnetNiger.Gateway/      # API Gateway centralisée
├── DotnetNiger.Identity/     # Service d'identité et authentification
├── DotnetNiger.Community/    # Service communautaire
└── docker-compose.yml        # Orchestration des services
```

### Services

| Service | Port | Description |
|---------|------|-------------|
| Gateway | 5000 | Point d'entrée unique |
| Identity | 5075 | Gestion des identités et JWT |
| Community | 5269 | Services communautaires |

## 🚀 Démarrage rapide

### Prérequis
- .NET 8 SDK ou supérieur
- Docker & Docker Compose
- Git

### Installation locale

```bash
# Cloner le repository
git clone <repository-url>
cd DotnetNiger

# Restaurer les dépendances
dotnet restore

# Lancer les migrations de base de données
dotnet ef database update

# Démarrer les services
dotnet run --project DotnetNiger.Gateway
dotnet run --project DotnetNiger.Identity
dotnet run --project DotnetNiger.Community
```

### Avec Docker Compose

```bash
# Construire et lancer les services
docker-compose up -d

# Vérifier le statut
docker-compose ps

# Arrêter les services
docker-compose down
```

## 📚 Documentation

- [SETUP.md](./SETUP.md) - Guide de configuration détaillé
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Architecture du projet
- [API.md](./API.md) - Documentation des APIs
- [DEPLOYMENT.md](./DEPLOYMENT.md) - Guide de déploiement
- [CONTRIBUTING.md](./CONTRIBUTING.md) - Guide pour les contributeurs

## 🔐 Accès à l'API

### Swagger/OpenAPI

Une fois les services lancés, accédez à la documentation interactive :

```
Gateway:    http://localhost:5000/swagger
Identity:   http://localhost:5075/swagger
Community:  http://localhost:5269/swagger
```

### Endpoints principaux

#### Gateway
- `GET /swagger` - Documentation Swagger
- `GET /health` - Health check
- `GET /metrics` - Métriques Prometheus

#### Identity Service
- `POST /auth/login` - Connexion
- `POST /auth/register` - Inscription
- `POST /auth/refresh` - Renouvellement du token
- `GET /auth/profile` - Profil utilisateur

#### Community Service
- `GET /posts` - Liste des posts
- `POST /posts` - Créer un post
- `GET /posts/{id}` - Détails d'un post

## 🔧 Configuration

### Fichiers de configuration

```
├── appsettings.json             # Configuration générale
├── appsettings.Development.json # Configuration développement
├── appsettings.Production.json  # Configuration production
└── Configuration/
    └── yarp-routes.json         # Configuration du routage
```

### Variables d'environnement

```env
# Authentification
JWT_SECRET=your-secret-key
JWT_ISSUER=https://Comming...
JWT_AUDIENCE=Comming...

# Base de données
DB_CONNECTION_STRING=Server=localhost;Database=DotnetNiger;User Id=sa;Password=YourPassword123!

# Services
IDENTITY_SERVICE_URL=http://localhost:5075
COMMUNITY_SERVICE_URL=http://localhost:5269

# Caching
REDIS_CONNECTION_STRING=localhost:6379

# Logging
LOG_LEVEL=Information
```

## 📊 Monitoring

### Healthchecks

```bash
# Gateway health
curl http://localhost:5000/health

# Identity health
curl http://localhost:5075/health

# Community health
curl http://localhost:5269/health
```

### Métriques Prometheus

```
http://localhost:5000/metrics
```

## 🧪 Tests

```bash
# Exécuter tous les tests
dotnet test

# Exécuter les tests d'un projet spécifique
dotnet test DotnetNiger.Gateway

# Avec rapport de couverture
dotnet test /p:CollectCoverage=true
```

## 📦 Build & Déploiement

### Build local

```bash
dotnet build -c Release
```

### Build Docker

```bash
docker build -t dotnetniger-gateway -f DotnetNiger.Gateway/Dockerfile .
docker build -t dotnetniger-identity -f DotnetNiger.Identity/Dockerfile .
docker build -t dotnetniger-community -f DotnetNiger.Community/Dockerfile .
```

### Déploiement

Consultez [DEPLOYMENT.md](./DEPLOYMENT.md) pour les instructions détaillées de déploiement en environnement de production.

## 🤝 Contribution

Les contributions sont bienvenues ! Consultez [CONTRIBUTING.md](./CONTRIBUTING.md) pour les directives.

### Processus de contribution

1. Fork le repository
2. Créer une branche pour votre feature (`git checkout -b feature/AmazingFeature`)
3. Commit vos changements (`git commit -m 'Add some AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

## 📝 Convention de code

- Respecter les directives [C# Coding Guidelines](https://docs.microsoft.com/en-us/dotnet/fundamentals/coding-style)
- Nommer les classes et méthodes en PascalCase
- Nommer les variables locales en camelCase
- Ajouter des commentaires XML pour les méthodes publiques
- Maintenir les tests unitaires pour le code critique

## 🐛 Signalement de bugs

Pour signaler un bug, veuillez ouvrir une issue avec :
- Description détaillée du problème
- Étapes pour reproduire
- Comportement attendu vs actuel
- Logs d'erreur pertinents
- Environnement (OS, versions, etc.)

## 📄 Licences

Ce projet est sous licence MIT. Voir [LICENSE](./LICENSE) pour les détails.

## 👥 Auteurs

- **DotnetNiger Team** - Travail initial

## 🙏 Remerciements

- [YARP](https://microsoft.github.io/reverse-proxy/) - Reverse proxy
- [Polly](https://github.com/App-vNext/Polly) - Circuit breaker
- [Serilog](https://serilog.net/) - Logging
- [Prometheus](https://prometheus.io/) - Monitoring

## 📞 Support

Pour toute question ou problème :
- Consulter la [documentation](./docs)
- Ouvrir une issue sur GitHub
- Contacter l'équipe de support

## 🗺️ Roadmap

- [ ] Implémentation complète de l'authentification JWT
- [ ] Services de notification email/SMS
- [ ] Dashboard d'administration
- [ ] Analytics et rapports
- [ ] Support multi-tenancy
- [ ] API GraphQL
- [ ] Authentification OAuth2/OIDC

## 📊 Statistiques

- **Langage**: C# (.NET 9)
- **Architecture**: Microservices
- **Pattern**: Clean Architecture
- **Database**: SQL Server/Sqlite (test local)
- **Cache**: Redis
- **Monitoring**: Prometheus
- **Container**: Docker

---

**Dernière mise à jour**: 29 Janvier 2026
