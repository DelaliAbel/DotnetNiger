# DotnetNiger API Gateway

## Vue d'ensemble

Le **DotnetNiger API Gateway** est un reverse proxy construit avec **YARP** (Yet Another Reverse Proxy) qui agrège et redirige les requêtes vers les services microservices de l'écosystème DotnetNiger.

### Fonctionnalités Principales

✅ **Reverse Proxy avec YARP** - Routage intelligent des requêtes  
✅ **Clients API Typés** - Communication type-safe avec les services  
✅ **Caching Intelligent** - Mise en cache automatique des réponses  
✅ **Rate Limiting** - Protection contre les abus  
✅ **Authentification** - Support des tokens JWT  
✅ **Métriques et Monitoring** - Suivi des performances  
✅ **Swagger Agrégé** - Documentation unifiée de tous les services  

## Architecture de Routage

```
Client → Gateway (5000)
            ├─ /identity/* → Identity Service (5075)
            ├─ /community/* → Community Service (5269)
            ├─ /api/gateway/* → Endpoints Gateway
            └─ /swagger/* → Documentation
```

## Installation et Configuration

### Prérequis

- .NET 8.0 SDK ou supérieur
- Visual Studio Code ou Visual Studio
- Services Identity et Community en mouvement

### Démarrage Rapide

#### 1. Lancer les Services (Terminal 1)
```bash
# Identity Service
cd DotnetNiger.Identity
dotnet run --launch-profile http

# Dans un nouveau terminal (Terminal 2)
# Community Service
cd DotnetNiger.Community
dotnet run --launch-profile http
```

#### 2. Lancer le Gateway (Terminal 3)
```bash
cd DotnetNiger.Gateway
dotnet run --launch-profile http
```

Le Gateway sera accessible à:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Swagger Agrégé**: http://localhost:5000/swagger-aggregated

### Avec Docker

```bash
# Builder l'image
docker build -t dotnetniger-gateway .

# Lancer le container
docker run -p 5000:8080 \
  -e IDENTITY_SERVICE_URL=http://identity-service:5000 \
  -e COMMUNITY_SERVICE_URL=http://community-service:5000 \
  dotnetniger-gateway
```

## Configuration

### appsettings.json
```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/identity/{**catch-all}" }
      },
      "community-route": {
        "ClusterId": "community-cluster",
        "Match": { "Path": "/community/{**catch-all}" }
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5075/"
          }
        }
      },
      "community-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5269/"
          }
        }
      }
    }
  }
}
```

### Ports

| Service | Port | Notes |
|---------|------|-------|
| Gateway | 5000 | Service principal |
| Identity | 5075 | Authentification et gestion des utilisateurs |
| Community | 5269 | Contenu et interactions |

## Endpoints Gateway

### Health Check

```bash
GET http://localhost:5000/api/gateway/status
```

### Routage vers Identity
```bash
# Via Gateway (se termine en erreur si non authentifié via YARP)
GET  /identity/api/users/me
POST /identity/api/auth/login
GET  /identity/api/roles
```

### Routage vers Community
```bash
GET  /community/api/posts?page=1&pageSize=10
GET  /community/api/events
GET  /community/api/projects
```

### Endpoints d'Agrégation du Gateway
```bash
# Récupérer les posts avec cache automatique
GET  /api/gateway/community/posts

# Récupérer les événements
GET  /api/gateway/community/events

# Recherche globale
GET  /api/gateway/search?query=dotnet

# Authentification via le gateway
POST /api/gateway/identity/auth/login
GET  /api/gateway/identity/users/me
```

### Documentation
```bash
# Gateway Swagger
GET  /swagger

# Swagger Agrégé (tous les services)
GET  /swagger-aggregated/v1/swagger.json

# Services individuels
GET  /swagger/identity/v1/swagger.json
GET  /swagger/community/v1/swagger.json
```

## Clients API Typés

### IIdentityApiClient
```csharp
var user = await _identityClient.GetCurrentUserAsync(token);
var roles = await _identityClient.GetRolesAsync();
var auth = await _identityClient.AuthenticateAsync(loginRequest);
```

### ICommunityApiClient
```csharp
var posts = await _communityClient.GetPostsAsync(page: 1, pageSize: 10);
var events = await _communityClient.GetEventsAsync();
var search = await _communityClient.SearchAsync("dotnet");
```

## Services Disponibles

### IRequestForwardingService
Relaie les requêtes HTTP vers les services microservices

### IRouteService
Détermine le service cible basé sur le chemin

### IAuthenticationService
Gère l'authentification et la validation des tokens JWT

### ICachingService
Met en cache les réponses (défaut: 1 heure)

### IRateLimitService
Limite les requêtes par client (défaut: 100/minute)

### IMetricsService
Collecte les métriques de performance

## Tests

### Avec le fichier .http

```bash
# Ouvrir DotnetNiger.Gateway.http dans VS Code
# Utiliser l'extension REST Client pour tester les endpoints
```

### Avec curl

```bash
# Login
curl -X POST http://localhost:5000/identity/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Get Posts
curl -X GET "http://localhost:5000/community/api/posts?page=1&pageSize=10"

# Search
curl -X GET "http://localhost:5000/api/gateway/search?query=dotnet"
```

### Avec Postman

1. Importer la collection `DotnetNiger.Gateway.http`
2. Configurer l'environnement avec `baseUrl = http://localhost:5000`
3. Exécuter les requêtes de test

## Structure du Projet

```
DotnetNiger.Gateway/
├── Api/
│   ├── Controllers/
│   │   ├── GatewayController.cs      # Endpoints du gateway
│   │   └── SwaggerAggregatorController.cs
│   ├── Extensions/
│   │   ├── ServiceExtensions.cs      # Enregistrement des services
│   │   └── MiddlewareExtensions.cs
│   └── Filters/
│       ├── ExceptionFilter.cs
│       └── ValidationFilter.cs
├── Application/
│   ├── DTOs/
│   ├── Services/
│   │   ├── RequestForwardingService.cs
│   │   ├── RouteService.cs
│   │   ├── AuthenticationService.cs
│   │   ├── CachingService.cs
│   │   ├── RateLimitService.cs
│   │   └── MetricsService.cs
│   └── Services/Interfaces/
├── Infrastructure/
│   ├── HttpClients/
│   │   ├── ApiClientBase.cs
│   │   ├── IIdentityApiClient.cs
│   │   ├── IdentityApiClient.cs
│   │   ├── ICommunityApiClient.cs
│   │   └── CommunityApiClient.cs
│   ├── Caching/
│   ├── Monitoring/
│   └── CircuitBreaker/
├── Configuration/
│   └── yarp-routes.json
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
├── Program.cs
└── DotnetNiger.Gateway.http
```

## Variables d'Environnement (Production)

```env
ASPNETCORE_ENVIRONMENT=Production
IDENTITY_SERVICE_URL=http://identity-service:5000
COMMUNITY_SERVICE_URL=http://community-service:5000
RATE_LIMIT_MAX_REQUESTS=100
CACHE_EXPIRATION_MINUTES=60
```

## Dépannage

### Les requêtes ne passent pas
- ✓ Vérifiez que Identity et Community s'exécutent sur les bons ports
- ✓ Vérifiez les logs du gateway: `grep -i "forwarding\|error" app.log`
- ✓ Testez: `curl http://localhost:5075/` (Identity accessible?)

### Swagger ne s'affiche pas
- ✓ Vérifiez: `ASPNETCORE_ENVIRONMENT=Development`
- ✓ Vérifiez que les services répondent à `/swagger/v1/swagger.json`

### Rate limiting trop strict
- ✓ Modifiez `RateLimit:MaxRequestsPerMinute` dans `appsettings.json`

### Erreurs de timeout
- ✓ Augmentez `Services:Identity:Timeout` et `Services:Community:Timeout`
- ✓ Vérifiez la connectivité réseau

## Performance et Production

### Optimisations Activées
- ✓ Caching en mémoire (défaut: 1 heure)
- ✓ Connection pooling pour HttpClient
- ✓ YARP load balancing
- ✓ Compression de réponses

### Monitoring
- Métriques collectées dans `IMetricsService`
- Logs structurés disponibles
- Health checks pour les services downstream

## Sécurité

- ✅ Authentification JWT supportée
- ✅ Validation des tokens
- ✅ Rate limiting (limit: 100 req/min)
- ✅ CORS configurable
- ✅ HTTPS enforced en production

## Documentation Complète

Pour plus de détails sur le routage:
Voir → [GATEWAY_ROUTING.md](../docs/GATEWAY_ROUTING.md)

## Contribution

Pour contribuer au Gateway:
1. Créer une branche: `git checkout -b feature/ma-feature`
2. Committer: `git commit -am 'Ajouter ma feature'`
3. Pousser: `git push origin feature/ma-feature`
4. Créer une Pull Request

## Support

- 📧 Issues: [GitHub Issues](https://github.com/DotnetNiger/DotnetNiger/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/DotnetNiger/DotnetNiger/discussions)
- 🌐 Website: [DotnetNiger.dev](https://dotnetniger.dev)

## License

MIT License - voir [LICENSE.md](../LICENSE.md)

---

**Version**: 1.0  
**Dernière mise à jour**: Février 2026  
**Mainteneur**: DotnetNiger Community
