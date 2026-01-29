# Architecture du Projet

## 📐 Vue d'ensemble

DotnetNiger suit une architecture de **microservices** avec une **API Gateway** centralisée. Cette approche permet :

- Scalabilité indépendante des services
- Déploiement isolé
- Technologies flexibles par service
- Résilience améliorée
- Équipes de développement décentralisées

## 🏗️ Architecture générale

```
┌─────────────────────────────────────────────────────────────┐
│                      Clients (Web, Mobile)                   │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTP/HTTPS
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                     DotnetNiger.Gateway                      │
│                                                              │
│  - YARP (Reverse Proxy)                                     │
│  - Rate Limiting                                            │
│  - Authentication/JWT                                      │
│  - Request/Response Transform                              │
│  - Logging & Monitoring                                    │
│  - Circuit Breaker                                         │
│  - Caching                                                 │
└────┬─────────────────────┬────────────────────┬────────────┘
     │                     │                    │
     ▼                     ▼                    ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Identity    │  │  Community   │  │  [Services]  │
│  Service     │  │  Service     │  │              │
│              │  │              │  │              │
│ - Auth       │  │ - Posts      │  │ - [TBD]      │
│ - Users      │  │ - Comments   │  │              │
│ - Tokens     │  │ - Followers  │  │              │
│ - Profiles   │  │ - Feed       │  │              │
└──────────────┘  └──────────────┘  └──────────────┘
     │                    │                    │
     └────────┬───────────┴────────┬───────────┘
              │                    │
              ▼                    ▼
        ┌──────────────┐    ┌──────────────┐
        │  SQL Server  │    │    Redis     │
        │  (Database)  │    │   (Cache)    │
        └──────────────┘    └──────────────┘
```

## 📦 Structure des services

Chaque service suit la **Clean Architecture** :

```
Service/
├── Api/                          # Couche présentation
│   ├── Controllers/              # Endpoints
│   ├── Middleware/               # Pipelines HTTP
│   ├── Filters/                  # Exception/Validation
│   └── Extensions/               # Configuration
│
├── Application/                  # Logique métier
│   ├── Services/                 # Business logic
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Mappers/                  # Mapping objets
│   ├── Validators/               # Validation métier
│   └── Exceptions/               # Custom exceptions
│
├── Domain/                       # Entités métier
│   ├── Entities/                 # Modèles métier
│   ├── Enums/                    # Énumérations
│   └── Interfaces/               # Contrats
│
├── Infrastructure/               # Détails techniques
│   ├── Data/                     # DbContext, Migrations
│   ├── Repositories/             # Accès données
│   ├── Caching/                  # Cache
│   ├── External/                 # Services externes
│   └── Security/                 # Authentification
│
├── Program.cs                    # Point d'entrée
├── appsettings.json              # Configuration
└── Dockerfile                    # Containerisation
```

## 🌐 DotnetNiger.Gateway

### Responsabilités

1. **Reverse Proxy (YARP)**
   - Route les requêtes vers les services appropriés
   - Gère les règles de routage complexes
   - Support du load balancing

2. **Authentication & Authorization**
   - Valide les tokens JWT
   - Injecte le contexte utilisateur
   - Gère les autorisations

3. **Rate Limiting**
   - Limite les requêtes par client
   - Protège contre les abus
   - Configurable par endpoint

4. **Caching**
   - Cache les réponses GET
   - Réduit la charge sur les services
   - Expire automatiquement

5. **Monitoring**
   - Logs centralisés (Serilog)
   - Métriques Prometheus
   - Health checks

6. **Resilience**
   - Circuit breaker (Polly)
   - Retry logic
   - Timeouts configurables

### Stack technologique

```json
{
  "dependencies": {
    "YARP": "2.1.0",
    "Polly": "8.2.0",
    "Serilog": "3.1.1",
    "Prometheus": "1.5.0",
    "StackExchange.Redis": "2.6.0"
  }
}
```

## 🔐 DotnetNiger.Identity

### Responsabilités

1. **Authentification**
   - Login/Logout
   - Génération JWT
   - Refresh tokens

2. **Gestion des utilisateurs**
   - Registration
   - Profil utilisateur
   - Password management

3. **Sécurité**
   - Hash de passwords (bcrypt)
   - MFA (optionnel)
   - Audit des connexions

### Entités principales

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
```

## 👥 DotnetNiger.Community

### Responsabilités

1. **Gestion du contenu**
   - Posts/Articles
   - Commentaires
   - Likes/Dislikes

2. **Interactions sociales**
   - Follow/Unfollow
   - Notifications
   - Feed utilisateur

3. **Modération**
   - Signalement de contenu
   - Suppression
   - Blocage d'utilisateurs

### Entités principales

```csharp
public class Post
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## 🔄 Patterns et Pratiques

### 1. Repository Pattern

```csharp
// Interface
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

// Implementation
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    // ...autres méthodes
}
```

### 2. Service Layer

```csharp
public interface IUserService
{
    Task<UserDto> GetUserAsync(int id);
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task UpdateUserAsync(int id, UpdateUserRequest request);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        _logger.LogInformation("Récupération de l'utilisateur {UserId}", id);
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException($"Utilisateur {id} non trouvé");

        return _mapper.Map<UserDto>(user);
    }
}
```

### 3. Dependency Injection

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddLogging();

var app = builder.Build();
```

### 4. Error Handling

```csharp
// Custom Exceptions
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message)
        : base(message, 404) { }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message)
        : base(message, 401) { }
}

// Middleware
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApplicationException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsJsonAsync(
                new { error = ex.Message });
        }
    }
}
```

## 📊 Flux de données

### Authentication Flow

```
1. Client
   │
   └─── POST /auth/login ──┐
                           │
2. Gateway                 │
   ├─ Validate JWT ────────┤
   ├─ Rate Limiting ───────┤
   └─ Forward ────────────┐│
                          ││
3. Identity Service       ││
   ├─ Verify credentials ─┘│
   ├─ Generate JWT ───────┐│
   └─ Return token ───┐   ││
                      │   ││
4. Gateway            │   ││
   ├─ Cache token ────┼─┐ ││
   └─ Return ─────────┼─┼─┘│
                      │ │  │
5. Client             │ │  │
   ├─ Store token ────┘ │  │
   └─ Send with X-Auth  │  │
      headers ──────────┘──┘
```

### Request Flow

```
Client
  │
  ├─ GET /community/posts
  │
Gateway
  ├─ Check cache ──────────► Redis (cache hit)
  ├─ Validate token ───────► JWT valid
  ├─ Rate limit check ─────► OK
  ├─ Route to service ─────┐
  │                        │
Community Service         │
  ├─ Authorize user ──────┘
  ├─ Fetch posts ─────────┐
  │                       │
Database                  │
  └─ Return posts ────────┘
  │
Gateway
  ├─ Cache response ──────► Redis
  └─ Return to client
  │
Client
  └─ Receive response
```

## 🔐 Sécurité

### JWT Token Structure

```
Header.Payload.Signature

Payload:
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "user",
  "iat": 1234567890,
  "exp": 1234571490
}
```

### Authentification

1. **Token Validation** - Signature, expiration, issuer
2. **Rate Limiting** - Protection contre brute force
3. **HTTPS** - Chiffrement en transit
4. **Password Hashing** - bcrypt avec salt

### CORS Policy

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

## 📈 Scalabilité

### Horizontal Scaling

Chaque service peut être déployé indépendamment :

```yaml
services:
  gateway:
    replicas: 3

  identity:
    replicas: 2

  community:
    replicas: 3
```

### Caching Strategy

- **L1 Cache** - Application memory (MemoryCache)
- **L2 Cache** - Redis (distributed)
- **Cache Invalidation** - TTL + événements

### Database

- **Read Replicas** - Pour les lectures intensives
- **Connection Pooling** - Optimiser les connexions
- **Indexes** - Sur les colonnes fréquemment queryées

## 🚀 Déploiement

### Docker

Chaque service a son Dockerfile multi-stage :

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/bin/Release ./
ENTRYPOINT ["dotnet", "DotnetNiger.Gateway.dll"]
```

### Orchestration

```yaml
version: "3.8"
services:
  gateway:
    image: dotnetniger-gateway:latest
    ports:
      - "5000:80"
    depends_on:
      - identity
      - community
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

---

Pour plus de détails, consulter les fichiers de chaque service.
