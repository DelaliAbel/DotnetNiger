# Gateway - Redirection et Communication inter-Services

## Vue d'ensemble

Le **DotnetNiger API Gateway** est un reverse proxy qui agrège et redirige les requêtes vers les services microservices (Identity et Community). Il utilise **YARP** (Yet Another Reverse Proxy) pour gérer les routes.

## Architecture

```
Client Requests
    ↓
Gateway (Port 5000)
    ├── YARP Routes
    ├── Services Clients
    ├── Caching
    ├── Rate Limiting
    └── Authentication
         ↓
    ├─→ Identity Service (Port 5075)
    ├─→ Community Service (Port 5269)
    └─→ Swagger Aggregator
```

## Routes Configurées

### 1. Routes Identity Service

- **Path Pattern**: `/identity/{**catch-all}`
- **Cluster**: `identity-cluster`
- **Destination**: `http://localhost:5075/`

**Exemples d'utilisation**:
```
GET  /identity/api/users/me                    → Forward to Identity Service
POST /identity/api/auth/login                  → Forward to Identity Service
GET  /identity/swagger/v1/swagger.json         → Swagger spec de Identity
```

### 2. Routes Community Service

- **Path Pattern**: `/community/{**catch-all}`
- **Cluster**: `community-cluster`
- **Destination**: `http://localhost:5269/`

**Exemples d'utilisation**:
```
GET  /community/api/posts                      → Forward to Community Service
GET  /community/api/events                     → Forward to Community Service
GET  /community/api/projects                   → Forward to Community Service
POST /community/api/search                     → Search across Community Service
```

### 3. Routes Swagger Aggregées

- **Swagger Gateway**: `/swagger/v1/swagger.json`
- **Swagger Agrégé**: `/swagger-aggregated/v1/swagger.json`
- **Swagger Identity**: `/swagger/identity/v1/swagger.json`
- **Swagger Community**: `/swagger/community/v1/swagger.json`

## Client API Implémentations

### IIdentityApiClient
Fournit des méthodes typées pour communiquer avec le service Identity:

```csharp
// Authentification
Task<AuthResponse?> AuthenticateAsync(LoginRequest request);
Task<AuthResponse?> RegisterAsync(RegisterRequest request);
Task<AuthResponse?> RefreshTokenAsync(string refreshToken);

// Utilisateurs
Task<UserInfoResponse?> GetCurrentUserAsync(string token);
Task<UserInfoResponse?> GetUserByIdAsync(string userId);

// Rôles et Tokens
Task<List<RoleResponse>?> GetRolesAsync();
Task<bool> ValidateTokenAsync(string token);
```

### ICommunityApiClient
Fournit des méthodes typées pour communiquer avec le service Community:

```csharp
// Posts
Task<List<PostResponse>?> GetPostsAsync(int page = 1, int pageSize = 10);
Task<PostResponse?> GetPostByIdAsync(string postId);
Task<PostResponse?> CreatePostAsync(CreatePostRequest request);

// Événements
Task<List<EventResponse>?> GetEventsAsync();
Task<EventResponse?> GetEventByIdAsync(string eventId);

// Projets et Catégories
Task<List<ProjectResponse>?> GetProjectsAsync();
Task<List<CategoryResponse>?> GetCategoriesAsync();

// Recherche
Task<SearchResponse?> SearchAsync(string query);
```

## Services du Gateway

### 1. RequestForwardingService (IRequestForwardingService)
Relaie les requêtes HTTP vers les services microservices.

```csharp
Task<HttpResponseMessage> ForwardRequestAsync(string serviceUrl, HttpRequest request);
```

### 2. RouteService (IRouteService)
Détermine le service cible basé sur le chemin de la requête.

```csharp
string DetermineServiceRoute(string path);    // Retourne "identity" ou "community"
bool IsValidRoute(string path);               // Valide si la route existe
```

### 3. AuthenticationService (IAuthenticationService)
Gère l'authentification et la validation des tokens JWT.

```csharp
Task<bool> ValidateTokenAsync(string token);
Task<string?> GetUserIdFromTokenAsync(string token);
```

### 4. CachingService (ICachingService)
Met en cache les réponses des services pour améliorer les performances.

```csharp
Task<T?> GetAsync<T>(string key);
Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
Task RemoveAsync(string key);
```

### 5. RateLimitService (IRateLimitService)
Limite le nombre de requêtes par client pour éviter l'abus.

```csharp
Task<bool> IsRequestAllowedAsync(string clientId, string endpoint);
Task IncrementRequestCountAsync(string clientId, string endpoint);
```

### 6. MetricsService (IMetricsService)
Collecte les métriques de performance et les erreurs.

```csharp
void RecordRequest(string endpoint, string method);
void RecordResponseTime(string endpoint, long milliseconds);
void RecordError(string endpoint, string errorType);
```

## Endpoints du Gateway

### Santé et Statut

```
GET /api/gateway/status
```

Réponse:
```json
{
  "status": "Gateway is running",
  "timestamp": "2026-02-19T10:30:00Z",
  "services": {
    "identity": "Available at /identity",
    "community": "Available at /community",
    "swagger": "Available at /swagger"
  }
}
```

### Identity Endpoints (via Gateway)

```
POST /api/gateway/identity/auth/login
GET  /api/gateway/identity/users/me
```

### Community Endpoints (via Gateway)

```
GET /api/gateway/community/posts
GET /api/gateway/community/events
GET /api/gateway/community/projects
```

### Recherche Globale

```
GET /api/gateway/search?query=terme
```

### Statistiques

```
GET /api/gateway/stats
```

## Configuration en Production

Pour la production, mettez à jour `appsettings.Production.json`:

```json
{
  "ReverseProxy": {
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://identity-service:5000/"
          }
        }
      },
      "community-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://community-service:5000/"
          }
        }
      }
    }
  }
}
```

## Middleware et Ordre de Traitement

1. **HTTPS Redirection**
2. **CORS** - Autoriser les requêtes cross-origin
3. **Authorization**
4. **Route Matching**
5. **YARP Reverse Proxy**

## Exemple d'Utilisation - Depuis une Application Client

### Via requête REST

```bash
# Login
curl -X POST http://localhost:5000/identity/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Get current user
curl -X GET http://localhost:5000/identity/api/users/me \
  -H "Authorization: Bearer YOUR_TOKEN"

# Get posts
curl -X GET http://localhost:5000/community/api/posts?page=1&pageSize=10

# Search
curl -X GET http://localhost:5000/api/gateway/search?query=dotnet
```

### Via les clients typés (Services internes)

```csharp
public class MyService
{
    private readonly IIdentityApiClient _identityClient;
    private readonly ICommunityApiClient _communityClient;

    public async Task DoSomething()
    {
        // Obtenir l'utilisateur courant
        var user = await _identityClient.GetCurrentUserAsync(token);

        // Récupérer les posts et les mettre en cache
        var posts = await _communityClient.GetPostsAsync(page: 1, pageSize: 10);

        // Rechercher du contenu
        var results = await _communityClient.SearchAsync("dotnet");
    }
}
```

## Démarrage du Gateway

```bash
# En développement
dotnet run --project DotnetNiger.Gateway

# Ou via le script
./run.ps1  # Windows
./run.sh   # Linux/Mac
```

Le gateway sera accessible à:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Swagger Agrégé**: http://localhost:5000/swagger-aggregated

## Logs Importants

Regardez les logs pour:

```
[Information] Forwarding request to: http://localhost:5075/api/users/me
[Warning] Route inconnue: /unknown
[Error] Erreur lors de la requête...
```

## Troubleshooting

### Les requêtes ne passent pas
- Vérifiez que les services Identity et Community s'exécutent sur les bons ports
- Vérifiez les logs du gateway pour les erreurs
- Confirmez que `appsettings.json` a les bonnes adresses

### Swagger ne s'affiche pas
- Assurez-vous que le Gateway s'exécute en développement (`ASPNETCORE_ENVIRONMENT=Development`)
- Vérifiez que les services sont accessibles

### Rate limiting activation
- Configurez `RateLimit:MaxRequestsPerMinute` dans `appsettings.json`
- Les clients sont identifiés par leur adresse IP par défaut

## Sécurité

- ✅ Authentification JWT supportée
- ✅ Validation des tokens
- ✅ Rate limiting pour prévenir l'abus
- ✅ CORS configurable
- ✅ HTTPS redirection activée

---

**Version**: 1.0  
**Dernière mise à jour**: Février 2026
