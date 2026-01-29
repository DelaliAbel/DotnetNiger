# 📂 Structure du Projet

Vue complète de l'organisation des fichiers et dossiers.

## 📋 Racine du Projet

```
DotnetNiger/
├── 📁 docs/                          # 📚 Documentation (ce fichier)
├── 📁 DotnetNiger.Gateway/           # 🚪 API Gateway
├── 📁 DotnetNiger.Identity/          # 🔐 Service Identity
├── 📁 DotnetNiger.Community/         # 👥 Service Community
├── docker-compose.yml                # 🐳 Orchestration
├── DotnetNiger.slnx                  # 📦 Solution file
├── VERSION                           # 🏷️ Version courante
├── README.md                         # 📖 Accueil projet
├── LICENSE.md                        # ⚖️ Licence
├── CODE_OF_CONDUCT.md                # 👨‍⚖️ Code de conduite
└── ... autres fichiers doc

```

## 🚪 DotnetNiger.Gateway

```
DotnetNiger.Gateway/
├── 📁 Api/
│   ├── Controllers/                  # Endpoints
│   │   ├── HealthController.cs       # Health checks
│   │   ├── SwaggerAggregatorController.cs  # Swagger aggregation
│   │   └── MetricsController.cs      # Prometheus metrics
│   ├── Middleware/                   # Pipeline HTTP
│   │   ├── RateLimitingMiddleware.cs
│   │   ├── AuthenticationMiddleware.cs
│   │   ├── JwtInjectionMiddleware.cs
│   │   ├── RequestLoggingMiddleware.cs
│   │   ├── ErrorHandlingMiddleware.cs
│   │   ├── CorsMiddleware.cs
│   │   └── RequestTransformMiddleware.cs
│   ├── Filters/
│   │   ├── ExceptionFilter.cs
│   │   └── ValidationFilter.cs
│   └── Extensions/
│       ├── ServiceExtensions.cs
│       └── MiddlewareExtensions.cs
│
├── 📁 Application/
│   ├── Services/
│   │   ├── SwaggerAggregatorService.cs
│   │   ├── RateLimitService.cs
│   │   ├── AuthService.cs
│   │   ├── ForwardingService.cs
│   │   ├── CachingService.cs
│   │   ├── CircuitBreakerService.cs
│   │   ├── MetricsService.cs
│   │   ├── LoadBalancerService.cs
│   │   ├── RequestCorrelationService.cs
│   │   ├── LoggingService.cs
│   │   ├── HealthCheckService.cs
│   │   └── TransformationService.cs
│   ├── DTOs/
│   │   ├── Requests/
│   │   │   └── ForwardRequest.cs
│   │   └── Responses/
│   │       ├── GatewayResponse.cs
│   │       ├── ErrorResponse.cs
│   │       └── MetricsDto.cs
│   └── Exceptions/
│       ├── GatewayException.cs
│       ├── ServiceUnavailableException.cs
│       ├── RateLimitExceededException.cs
│       ├── InvalidTokenException.cs
│       └── CircuitBreakerOpenException.cs
│
├── 📁 Infrastructure/
│   ├── Config/
│   │   ├── GatewayConfig.cs
│   │   ├── RateLimitConfig.cs
│   │   ├── ServiceRegistry.cs
│   │   └── RouteConfiguration.cs
│   ├── HttpClients/
│   │   ├── CommunityServiceClient.cs
│   │   ├── IdentityServiceClient.cs
│   │   ├── BaseServiceClient.cs
│   │   ├── IServiceClient.cs
│   │   └── HttpClientFactory.cs
│   ├── Caching/
│   │   ├── ICacheProvider.cs
│   │   └── RedisCacheProvider.cs
│   ├── Monitoring/
│   │   ├── HealthCheckService.cs
│   │   ├── MetricsCollector.cs
│   │   ├── CircuitBreakerOptions.cs
│   │   └── PrometheusMetrics.cs
│   └── CircuitBreaker/
│       ├── CircuitBreakerPolicy.cs
│       └── PollyConfiguration.cs
│
├── 📁 Configuration/
│   └── yarp-routes.json              # Routes YARP
│
├── Program.cs                        # Point d'entrée
├── appsettings.json                  # Config prod
├── appsettings.Development.json      # Config dev
├── appsettings.Production.json       # Config prod
├── DotnetNiger.Gateway.csproj        # Project file
├── DotnetNiger.Gateway.csproj.user   # User config
├── Dockerfile                        # Image Docker
└── DotnetNiger.Gateway.http          # Tests HTTP

```

## 🔐 DotnetNiger.Identity

```
DotnetNiger.Identity/
├── 📁 Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   └── ProfilesController.cs
│   ├── Middleware/
│   └── Filters/
│
├── 📁 Application/
│   ├── Services/
│   │   ├── IAuthService.cs
│   │   ├── AuthService.cs
│   │   ├── IUserService.cs
│   │   ├── UserService.cs
│   │   ├── IJwtService.cs
│   │   ├── JwtService.cs
│   │   ├── IEmailService.cs
│   │   └── EmailService.cs
│   ├── DTOs/
│   │   ├── LoginDto.cs
│   │   ├── RegisterDto.cs
│   │   ├── TokenDto.cs
│   │   └── UserDto.cs
│   └── Validators/
│
├── 📁 Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── RefreshToken.cs
│   │   └── Role.cs
│   └── Enums/
│       └── UserRole.cs
│
├── 📁 Infrastructure/
│   ├── Data/
│   │   ├── IdentityDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── IUserRepository.cs
│   │   └── UserRepository.cs
│   ├── Security/
│   │   └── PasswordHasher.cs
│   └── External/
│       └── SendGridEmailProvider.cs
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
├── DotnetNiger.Identity.csproj
└── DotnetNiger.Identity.http
```

## 👥 DotnetNiger.Community

```
DotnetNiger.Community/
├── 📁 Api/
│   ├── Controllers/
│   │   ├── PostsController.cs
│   │   ├── CommentsController.cs
│   │   ├── InteractionsController.cs
│   │   └── FeedController.cs
│   ├── Middleware/
│   └── Filters/
│
├── 📁 Application/
│   ├── Services/
│   │   ├── IPostService.cs
│   │   ├── PostService.cs
│   │   ├── ICommentService.cs
│   │   ├── CommentService.cs
│   │   ├── IInteractionService.cs
│   │   ├── InteractionService.cs
│   │   ├── IFeedService.cs
│   │   ├── FeedService.cs
│   │   ├── IFollowService.cs
│   │   └── FollowService.cs
│   ├── DTOs/
│   │   ├── PostDto.cs
│   │   ├── CommentDto.cs
│   │   ├── FeedDto.cs
│   │   └── InteractionDto.cs
│   └── Validators/
│
├── 📁 Domain/
│   ├── Entities/
│   │   ├── Post.cs
│   │   ├── Comment.cs
│   │   ├── Like.cs
│   │   ├── Follow.cs
│   │   └── Feed.cs
│   └── Enums/
│       └── InteractionType.cs
│
├── 📁 Infrastructure/
│   ├── Data/
│   │   ├── CommunityDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── IPostRepository.cs
│   │   ├── PostRepository.cs
│   │   ├── ICommentRepository.cs
│   │   ├── CommentRepository.cs
│   │   ├── IInteractionRepository.cs
│   │   └── InteractionRepository.cs
│   ├── Caching/
│   │   └── CacheStrategy.cs
│   └── External/
│       └── ImageStorageProvider.cs
│
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
├── DotnetNiger.Community.csproj
└── DotnetNiger.Community.http
```

## 📚 Documentation

```
docs/
├── 00-INDEX.md                    # 📑 Index (ce fichier)
├── 01-SETUP.md                    # ⚙️ Installation
├── 02-QUICK-START.md              # 🚀 Démarrage rapide
├── 03-ARCHITECTURE.md             # 🏗️ Architecture
├── 04-TECHNICAL-STACK.md          # 🛠️ Stack technique
├── 05-PROJECT-STRUCTURE.md        # 📂 Structure (ce fichier)
├── 06-API.md                      # 📡 Documentation API
├── 07-INTEGRATION.md              # 🔗 Intégrations
├── 08-DOCKER.md                   # 🐳 Docker
├── 09-MONITORING.md               # 📊 Monitoring
├── 10-CONTRIBUTING.md             # 👨‍💻 Contribution
├── 11-CODE-STANDARDS.md           # 📋 Standards
├── 12-TESTING.md                  # 🧪 Testing
├── 13-ONBOARDING.md               # 👋 Onboarding
├── 14-FAQ.md                      # ❓ FAQ
├── 15-CHANGELOG.md                # 📝 Changelog
├── 16-SECURITY.md                 # 🔒 Sécurité
├── 17-CODE-OF-CONDUCT.md          # 👨‍⚖️ Code de conduite
└── 18-LICENSE.md                  # ⚖️ Licence
```

## 🔧 Fichiers de Configuration

```
Root/
├── .gitignore                     # Git exclusions
├── .dockerignore                  # Docker exclusions
├── .editorconfig                  # IDE configuration
├── .gitattributes                 # Line endings
├── docker-compose.yml             # Services orchestration
├── .github/                       # GitHub configuration
│   ├── workflows/
│   │   ├── tests.yml
│   │   ├── docker.yml
│   │   ├── sonar.yml
│   │   └── deploy.yml
│   ├── ISSUE_TEMPLATE/
│   ├── PULL_REQUEST_TEMPLATE.md
│   └── CODEOWNERS
└── VERSION                        # Versioning
```

---

## 📊 Statistiques

| Item                      | Count |
| ------------------------- | ----- |
| Services                  | 3     |
| Controllers               | 7+    |
| Services (Business Logic) | 12+   |
| DTOs                      | 15+   |
| Entities                  | 8+    |
| Repositories              | 5+    |
| Total .cs files           | 100+  |
| Documentation files       | 19    |

---

**Dernière mise à jour:** 29 Janvier 2026
