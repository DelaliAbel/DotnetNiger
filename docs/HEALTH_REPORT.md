# Rapport de Santé — DotnetNiger

> Analyse complète du projet — 19 Février 2026

## Score Global : 5/10

| Service | Avancement | Note |
|---|---|---|
| **Identity** | ~75% | Le plus avancé — API complète, services fonctionnels |
| **Gateway** | ~25% | YARP fonctionne, mais middleware/sécurité non câblé |
| **Community** | ~15% | Squelette posé, ~82% des fichiers sont vides |
| **Tests** | ~10% | Seulement 11 tests au total |

---

## 1. DotnetNiger.Identity — Bien avancé mais pas production-ready

### 1.1 Ce qui fonctionne bien

- 9 entités domaine complètes (`ApplicationUser`, `RefreshToken`, `LoginHistory`, `SocialLink`, `ApiKey`, `Permission`, `RolePermission`, `AdminActionLog`, `Role`)
- 10 controllers avec ~40 endpoints (Auth, Users, Admin, Roles, Permissions, SocialLinks, ApiKeys, Tokens, Diagnostics, Integrations)
- 11 services applicatifs tous fonctionnels
- 17 DTOs Request + 14 DTOs Response
- 5 validators FluentValidation
- Hiérarchie d'exceptions bien conçue (`IdentityException` → `InvalidCredentialsException`, `TokenExpiredException`, `UserAlreadyExistsException`, `UserNotFoundException`)
- Architecture Clean Architecture respectée
- Providers email multiples (SMTP, SendGrid, Mailgun)
- Upload d'avatar (local + Azure Blob Storage)
- API Key authentication avec handler dédié

### 1.2 Endpoints disponibles

| Controller | Route | Auth | Endpoints |
|---|---|---|---|
| **AuthController** | `/api/v1/auth` | Public | register, login, forgot-password, reset-password, verify-email, request-email-verification |
| **UsersController** | `/api/v1/users` | `[Authorize]` | GET/PUT /me, avatar CRUD, change-password, change-email |
| **TokensController** | `/api/v1/tokens` | `[Authorize]` | refresh (public), logout |
| **AdminController** | `/api/v1/admin` | `[Authorize(Roles = "Admin")]` | users list/detail/status, login-history, api-keys CRUD, audit-logs |
| **RolesController** | `/api/v1/roles` | `[Authorize(Roles = "Admin")]` | CRUD + assign/remove |
| **PermissionsController** | `/api/v1/permissions` | `[Authorize(Roles = "Admin")]` | CRUD + assign/remove |
| **SocialLinksController** | `/api/v1/social-links` | `[Authorize]` | GET, POST, DELETE |
| **ApiKeysController** | `/api/v1/api-keys` | `[Authorize]` | GET, POST, rotate, DELETE, revoke-all |
| **DiagnosticsController** | `/api/v1/diagnostics` | Public | ping, health |
| **IntegrationsController** | `/api/v1/integrations` | `[ApiKeyOnly]` | ping |

### 1.3 Ce qui manque / est incomplet

| Problème | Sévérité | Détail |
|---|---|---|
| 6 fichiers Enums vides | Moyenne | `PlatformType.cs`, `RoleType.cs`, `UserStatus.cs` — seulement des headers de commentaire |
| 3 interfaces domaine vides | Moyenne | `IAuditableEntity.cs`, `ITokenable.cs`, `IUser.cs` — aucun contrat défini |
| 3 seeders vides | **Haute** | `DefaultRolesSeeder.cs`, `PermissionSeeder.cs`, `RoleSeeder.cs` — pas de données initiales |
| `DotnetNigerIdentityDbFactory` vide | Basse | Pas de design-time factory pour les migrations EF |
| `JwtMiddleware` est un pass-through | Basse | Ne fait rien — le code est un simple `_next(context)` |
| `MappingProfile` vide | Moyenne | Pas de profil AutoMapper → mapping dupliqué dans 3 services |
| `AvatarCleanupService` entièrement commenté | Basse | Service de nettoyage d'avatars orphelins désactivé |
| Seed admin dans `Program.cs` commenté | **Haute** | Impossible de créer le premier admin sans intervention manuelle |
| Repositories enregistrés mais jamais utilisés | Moyenne | `UserRepository`, `RefreshTokenRepository`, `LoginHistoryRepository` — code mort |
| `ICacheService`/`RedisCacheService` enregistrés mais jamais utilisés | Moyenne | Cache configuré mais jamais appelé par les services |
| `RequestEmailVerificationAsync` — envoi email commenté | **Haute** | La vérification email ne fonctionne pas réellement |
| `PasswordHasher`, `TokenValidator` — jamais utilisés | Basse | Code mort dans Infrastructure/Security |

### 1.4 Problèmes de sécurité

| Problème | Sévérité | Détail |
|---|---|---|
| Clé JWT placeholder dans appsettings | **CRITIQUE** | `"Key": "CHANGE_ME_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"` en clair dans le repo |
| API Key hashé sans sel | **Haute** | `ApiKeyHasher` utilise SHA256 simple → vulnérable aux rainbow tables |
| Refresh tokens stockés en clair | **Haute** | La colonne `Token` contient la valeur Base64 brute |
| Pas de rate-limiting | **Haute** | Login, register, forgot-password sans protection anti-brute-force |
| Pas de politique de lockout | Moyenne | `UserManager` lockout settings aux valeurs par défaut |
| Pas de CORS configuré | Moyenne | Aucun `AddCors()`/`UseCors()` dans Program.cs |
| Credentials email dans appsettings.Development.json | Moyenne | `"Username": "ton.email@gmail.com"`, `"Password": "CHANGE_ME"` |
| Admin credentials en commentaires | Moyenne | `admin@dotnetniger.com` / `Admin2026@DotnetNiger` visible dans le code source |
| Clé de test trop courte | Basse | `"test-secret-key-1234567890"` (26 bytes < 32 requis pour HMAC-SHA256) |
| `ISystemClock` déprécié | Basse | Utilisé dans `ApiKeyAuthenticationHandler`, remplacé par `TimeProvider` en .NET 8 |

### 1.5 Problèmes de qualité de code

| Problème | Impact |
|---|---|
| Mapping `MapUserAsync()` dupliqué dans `AuthService`, `UserService`, `TokenService` | Maintenance difficile |
| Pas de propagation de `CancellationToken` | Requêtes non-annulables |
| `SaveChanges` appelé plusieurs fois par opération dans `AdminService` | Performance |
| `UpdateProfileAsync` efface les champs quand `null` est passé | Bug potentiel — partial update détruit les données |
| `LoginRequest.RememberMe` déclaré mais jamais utilisé | Code mort |
| `LoginHistory.Country`/`City` jamais remplis | Champs toujours null |
| `RefreshToken.IpAddress`/`UserAgent` jamais remplis | Champs toujours null |
| Commentaires mixtes français/anglais | Incohérence |
| Validators manquants pour `ChangePasswordRequest`, `ChangeEmailRequest`, `CreateApiKeyRequest`, `AddRoleRequest`, `AssignRoleRequest`, `AddPermissionRequest`, `AssignPermissionRequest` | Validation incomplète |
| `RegisterAsync` envoie le token brut dans le body email au lieu d'une URL | Non fonctionnel pour un frontend |
| Doublon ErrorHandlingMiddleware + ExceptionFilter | Redondance — les deux catchent `IdentityException` |
| 3 noms de connection string différents | Confusion config |

### 1.6 Scorecard Identity

| Domaine | Note | Commentaire |
|---|---|---|
| Entités/Domaine | 7/10 | Solides, mais enums et interfaces vides |
| API Design | 9/10 | Complète, versionnée, bien structurée |
| Services | 8/10 | Tous fonctionnels, mais code dupliqué |
| Sécurité | 5/10 | JWT en place, mais gestion des clés/secrets insuffisante |
| Infrastructure | 6/10 | Bonne fondation mais beaucoup de code mort |
| Tests | 3/10 | 11 tests seulement — lacunes majeures |
| Qualité de code | 6/10 | Propre mais mapping dupliqué, pas de CancellationToken |
| Production Readiness | 4/10 | Secrets, rate-limiting, CORS, seeds manquants |

---

## 2. DotnetNiger.Community — Stade de scaffolding

### 2.1 Ce qui est fait

- **14/16 entités** implémentées (Event, EventMedia, EventRegistration, Post, PostCategory, PostTag, Comment, Project, ProjectContributor, Resource, Partner, Category, Tag, TeamMember)
- **6/12 DTOs Request** implémentés (CreatePostRequest, CreateEventRequest, CreateProjectRequest, AddPartnerRequest, AddResourceRequest, RegisterEventRequest)
- **10/16 DTOs Response** implémentés (EventDto, EventMediaDto, PostDto, ProjectDto, ResourceDto, PartnerDto, CategoryDto, TagDto, TeamMemberDto, SocialLinkDto)
- `TestController` avec 2 endpoints de test (`GET /api/test/test`, `GET /api/test/health`)
- `Program.cs` minimal avec Swagger

### 2.2 Ce qui est VIDE (0 code)

| Couche | Fichiers vides | Détail |
|---|---|---|
| **Controllers** | 11/12 | EventsController, PostsController, ProjectsController, ResourcesController, PartnersController, CommentsController, CategoriesController, TagsController, TeamController, StatsController, SearchController |
| **Services** | 12/12 | EventService, PostService, ProjectService, ResourceService, PartnerService, CommentService, CategoryService, TagService, TeamService, StatisticsService, SearchService, IdentityApiClient |
| **Interfaces de service** | 12/12 | Tous les `IXxxService` correspondants |
| **Repositories** | 12/12 | BaseRepository, EventRepository, PostRepository, ProjectRepository, ResourceRepository, PartnerRepository + interfaces |
| **Validators** | 7/7 | CreateEventRequestValidator, CreatePostRequestValidator, CreateProjectRequestValidator, etc. |
| **Exceptions** | 5/5 | CommunityException, EventNotFoundException, PostNotFoundException, ResourceNotApprovedException, UnauthorizedAccessException |
| **Middleware** | 3/3 | ErrorHandlingMiddleware, JwtValidationMiddleware, RequestLoggingMiddleware |
| **Filters** | 4/4 | AuthorizeFilter, CacheFilter, ExceptionFilter, ValidateModelFilter |
| **Enums** | 8/8 | ApprovalStatus, EventType, PartnerLevel, PostType, RegistrationStatus, ResourceLevel, ResourceType, TeamPosition |
| **Interfaces domaine** | 3/3 | IEntity, IAuditableEntity, IPublishable |
| **DbContext** | 2/2 | CommunityDbContext, CommunityDbContextFactory |
| **Seeders** | 4/4 | DefaultDataSeeder, CategorySeeder, TagSeeder, PartnerSeeder |
| **Caching** | 3/3 | ICacheService, RedisCacheService, CacheKeyGenerator |
| **External Services** | 6/6 | ISearchProvider, ElasticsearchProvider, IFileUploadService, AzureBlobService, IIdentityApiClient, IdentityApiClient |
| **Extensions** | 2/2 | ServiceExtensions, MiddlewareExtensions |
| **Mapper** | 1/1 | MappingProfile |

### 2.3 Statistiques d'implémentation

| Couche | Fichiers | Implémentés | Vides | Taux |
|---|---|---|---|---|
| Domain/Entities | 16 | 14 | 2 | ~87% |
| Domain/Enums | 8 | 0 | 8 | 0% |
| Domain/Interfaces | 3 | 0 | 3 | 0% |
| Api/Controllers | 12 | 1 | 11 | ~8% |
| Api/Middleware | 3 | 0 | 3 | 0% |
| Api/Filters | 4 | 0 | 4 | 0% |
| Api/Extensions | 2 | 0 | 2 | 0% |
| Application/Services | 24 | 0 | 24 | 0% |
| Application/DTOs | 28 | 16 | 12 | ~57% |
| Application/Validators | 7 | 0 | 7 | 0% |
| Application/Exceptions | 5 | 0 | 5 | 0% |
| Infrastructure | 29 | 0 | 29 | 0% |
| Program.cs | 1 | 1 (minimal) | — | ~15% |
| **TOTAL** | **~140** | **~32** | **~108** | **~18%** |

### 2.4 Problèmes critiques

| Problème | Sévérité | Détail |
|---|---|---|
| Pas de DbContext | **CRITIQUE** | Aucune persistance possible |
| Pas de packages NuGet EF Core | **CRITIQUE** | Manque `Microsoft.EntityFrameworkCore`, provider SQLite/SqlServer |
| Pas d'authentification JWT | **CRITIQUE** | `UseAuthentication()` non appelé, aucun scheme |
| Pas de connection string | **CRITIQUE** | `appsettings.json` n'a que Logging/AllowedHosts |
| Pas de DI | **CRITIQUE** | `Program.cs` n'enregistre aucun service |
| Entities utilisent `string` au lieu d'enums | Moyenne | `EventType`, `PostType`, `Level` sont des `string` |
| `DateTime.UtcNow` dans les property initializers | Moyenne | Anti-pattern — le timestamp est celui de l'instanciation, pas de la persistence |
| Fichiers en doublon | Moyenne | `IIdentityApiClient` et `IdentityApiClient` existent dans 2 dossiers |
| `ResourceCategory` et `TeamMemberSkill` vides | Moyenne | Classes stub sans propriétés |
| Fichier `.http` référence `/weatherforecast` | Basse | Résidu de template .NET |
| Pas de FluentValidation dans les NuGet | Moyenne | Framework de validation non installé |
| Pas d'AutoMapper dans les NuGet | Basse | Librairie de mapping non installée |

---

## 3. DotnetNiger.Gateway — YARP fonctionne, le reste non

### 3.1 Ce qui fonctionne

- **YARP reverse proxy** configuré avec 5 routes
- **Swagger Aggregator** — merge les docs Swagger des 2 services en un document OpenAPI
- **Dockerfile** fonctionnel

### 3.2 Configuration YARP

| Route ID | Path Pattern | Cluster | Transform |
|---|---|---|---|
| `identity-uploads` | `/uploads/{**catch-all}` | identity-cluster | Aucun |
| `identity-route` | `/identity/{**catch-all}` | identity-cluster | Aucun |
| `identity-swagger` | `/swagger/identity/{**catch-all}` | identity-cluster | Rewrite vers `/swagger/{**catch-all}` |
| `community-route` | `/community/{**catch-all}` | identity-cluster | Aucun |
| `community-swagger` | `/swagger/community/{**catch-all}` | community-cluster | Rewrite vers `/swagger/{**catch-all}` |

> **⚠️ BUG :** `community-route` pointe vers `identity-cluster` au lieu de `community-cluster`

### 3.3 Middleware — câblé mais JAMAIS appelé

`app.UseGatewayMiddlewares()` et `builder.Services.AddGatewayServices()` ne sont **jamais appelés** dans `Program.cs`. Aucun des 7 middlewares ne s'exécute.

| Middleware | État | Détail |
|---|---|---|
| `ErrorHandlingMiddleware` | Implémenté, pas câblé | Catch exceptions → JSON 500. Expose `exception.Message` en prod |
| `RequestLoggingMiddleware` | Implémenté, pas câblé | Log method, path, status, durée |
| `RequestTransformMiddleware` | Implémenté, pas câblé | Ajoute `X-Gateway-Version` et `X-Request-Id` |
| `AuthenticationMiddleware` | **Stub**, pas câblé | Lit le header `Authorization` mais ne valide rien |
| `JwtInjectionMiddleware` | **Code mort**, pas câblé | Dépend de `AuthenticationMiddleware` qui ne fonctionne pas |
| `RateLimitingMiddleware` | **Stub**, pas câblé | Pass-through — "sera implémenté plus tard avec Redis" |
| `CorsMiddleware` | **Stub**, pas câblé | Pass-through |

### 3.4 Services applicatifs — implémentés mais non enregistrés en DI

| Service | Interface | État |
|---|---|---|
| `AuthenticationService` | `IAuthenticationService` | **Stub** — `ValidateTokenAsync` retourne toujours `true`, `GetUserIdFromTokenAsync` retourne `null` |
| `CachingService` | `ICachingService` | Implémenté (`IMemoryCache`) — jamais utilisé |
| `MetricsService` | `IMetricsService` | Minimal — log seulement — jamais utilisé |
| `RateLimitService` | `IRateLimitService` | **Cassé** — `Dictionary<string, int>` non thread-safe, pas de TTL |
| `RequestForwardingService` | `IRequestForwardingService` | Partiel — ne copie pas le body — jamais utilisé |
| `RouteService` | `IRouteService` | Implémenté — jamais utilisé (YARP gère le routing) |

### 3.5 Fichiers vides (19 fichiers)

| Catégorie | Fichiers |
|---|---|
| Exceptions (3) | `RateLimitExceededException`, `ServiceUnavailableException`, `UnauthorizedException` |
| Caching (2) | `ICacheProvider`, `RedisCacheProvider` |
| Circuit Breaker (2) | `ICircuitBreaker`, `PollyCircuitBreaker` |
| Config (4) | `GatewayConfig`, `RateLimitConfig`, `RouteConfiguration`, `ServiceRegistry` |
| HTTP Clients (5) | `ApiClientBase`, `CommunityApiClient`, `ICommunityApiClient`, `IdentityApiClient`, `IIdentityApiClient` |
| Monitoring (4) | `HealthCheckService`, `IHealthCheckService`, `IMetricsCollector`, `PrometheusMetricsCollector` |
| Configuration (1) | `yarp-routes.json` (fichier `{}` vide) |

### 3.6 Problèmes critiques Gateway

| Problème | Sévérité |
|---|---|
| **BUG : `community-route` pointe vers `identity-cluster`** | **CRITIQUE** |
| Aucune validation JWT au niveau Gateway | **CRITIQUE** |
| Pas de CORS | **Haute** |
| Pas de rate-limiting | **Haute** |
| Pas de health checks | **Haute** |
| Pas de config production (cluster addresses) | **Haute** |
| `RateLimitService` non thread-safe | Moyenne |
| `SwaggerAggregatorController` avale les exceptions silencieusement | Moyenne |
| `ErrorHandlingMiddleware` expose `exception.Message` en production | Moyenne |
| Pas de circuit breaker (Polly non installé) | Moyenne |
| Pas de tests | Moyenne |

### 3.7 Pipeline Program.cs — ce qui est et ce qui manque

**Ce qui est configuré :**
```
builder.Services.AddHttpClient()
builder.Services.AddControllers()
builder.Services.AddSwaggerGen(...)
builder.Services.AddReverseProxy().LoadFromConfig(...)

app.UseSwagger()           // dev only
app.UseSwaggerUI(...)      // dev only
app.UseHttpsRedirection()
app.UseAuthorization()     // no-op sans AddAuthorization + AddAuthentication
app.MapControllers()
app.MapReverseProxy()
```

**Ce qui manque :**
- `app.UseGatewayMiddlewares()` — jamais appelé
- `builder.Services.AddGatewayServices()` — jamais appelé
- `AddAuthentication()` / `AddJwtBearer()`
- `AddAuthorization()` avec policies
- `AddCors()` / `UseCors()`
- `AddHealthChecks()` / `MapHealthChecks()`
- `AddMemoryCache()` (requis pour `CachingService`)
- Enregistrement des services applicatifs
- Circuit breaker / resilience

---

## 4. Tests — Couverture minimale

| Projet | Tests | Ce qui est testé | Ce qui manque |
|---|---|---|---|
| **Identity Unit Tests** | 7 tests | Uniquement `UsersController` (avatar upload/delete) | AuthService, TokenService, AdminService, RoleService, PermissionService, SocialLinkService, ApiKeyService, LoginHistoryService, tous les autres controllers |
| **Identity Integration Tests** | 4 tests | Diagnostics (ping, health), Admin access (auth OK, forbid non-admin) | Auth flow complet, token refresh, user profile, roles, permissions, social links, API keys |
| **Community Tests** | 0 | — | **Aucun projet de test** |
| **Gateway Tests** | 0 | — | **Aucun projet de test** |

### Stack de test utilisée (Identity)
- xUnit
- Moq
- FluentAssertions
- `IdentityWebApplicationFactory` (SQLite in-memory)
- `TestUserFactory` (crée des users avec rôles + JWT)

---

## 5. Docker & Infrastructure

| Élément | État | Problème |
|---|---|---|
| `docker-compose.yml` | Fonctionnel | SQL Server commenté ; **`community-route` pointe vers `identity-cluster` (bug)** |
| Dockerfile Identity | OK | Multi-stage .NET 8, non-root `$APP_UID` |
| Dockerfile Community | OK | Multi-stage .NET 8 |
| Dockerfile Gateway | OK | Multi-stage .NET 8 |
| CI/CD | **Absent** | Pas de `.github/workflows`, pas de pipeline Azure DevOps |
| Gestion des secrets | **Absente** | Pas de `dotnet user-secrets`, pas de Azure Key Vault, secrets en dur |
| Health checks Docker | Définis | `curl` sur `/health` — mais l'endpoint n'existe pas sur Gateway et Community |
| Volumes | OK | `sqlserver_data` persistant |

---

## 6. Documentation

| Fichier | Contenu |
|---|---|
| `README.md` | Complet — architecture, setup, contribution guide |
| `docs/API.md` | Documentation API |
| `docs/ARCHITECTURE.md` | Architecture technique |
| `docs/SETUP.md` | Guide d'installation |
| `docs/INDEX.md` | Index de la documentation |
| `CHANGELOG.md` | Historique des changements |
| `SECURITY.md` | Politique de sécurité |
| `LICENSE.md` | Licence MIT |

> La documentation est **bien fournie** pour un projet en développement.

---

## 7. Plan d'action prioritaire

### Priorité CRITIQUE (à faire en premier)

| # | Action | Service | Impact |
|---|---|---|---|
| 1 | **Sécuriser les secrets** — externaliser la clé JWT, utiliser `dotnet user-secrets` ou variables d'environnement | Identity | Sécurité |
| 2 | **Fixer le bug `community-route`** — changer de `identity-cluster` à `community-cluster` dans `appsettings.json` | Gateway | Routing cassé |
| 3 | **Implémenter `CommunityDbContext`** + ajouter packages EF Core + connection string | Community | Débloque tout le développement |
| 4 | **Câbler les middlewares Gateway** — appeler `UseGatewayMiddlewares()` et `AddGatewayServices()` dans `Program.cs` | Gateway | Middleware mort |
| 5 | **Configurer CORS** sur les 3 services | Tous | Blocage frontend |
| 6 | **Activer le seed admin** dans Identity `Program.cs` | Identity | Pas d'admin possible |

### Priorité HAUTE

| # | Action | Service |
|---|---|---|
| 7 | Implémenter les controllers Community (commencer par `PostsController` et `EventsController`) | Community |
| 8 | Implémenter les services + repositories Community | Community |
| 9 | Ajouter l'authentification JWT au Gateway et à Community | Gateway, Community |
| 10 | Ajouter du rate-limiting sur login, register, forgot-password | Identity, Gateway |
| 11 | Hasher les refresh tokens (comme les API keys) | Identity |
| 12 | Ajouter les validators manquants (`ChangePasswordRequest`, `ChangeEmailRequest`, etc.) | Identity |
| 13 | Supprimer/utiliser le code mort (repositories, cache service, PasswordHasher, TokenValidator) | Identity |

### Priorité MOYENNE

| # | Action | Service |
|---|---|---|
| 14 | Créer des projets de test pour Community et Gateway | Community, Gateway |
| 15 | Augmenter la couverture de tests Identity (auth flow, admin, roles, permissions) | Identity |
| 16 | Implémenter les enums et migrer les entités de `string` vers les types enum | Community, Identity |
| 17 | Extraire le mapping dupliqué `MapUserAsync()` dans un mapper partagé | Identity |
| 18 | Implémenter le circuit breaker Polly dans le Gateway | Gateway |
| 19 | Configurer les health checks YARP sur les clusters | Gateway |
| 20 | Ajouter une pipeline CI/CD (GitHub Actions) | Tous |
| 21 | Implémenter les seeders (roles, permissions, catégories, tags) | Identity, Community |
| 22 | Ajouter `appsettings.Production.json` avec les addresses de cluster | Gateway |

### Priorité BASSE

| # | Action | Service |
|---|---|---|
| 23 | Implémenter le caching Redis | Identity, Community |
| 24 | Implémenter la recherche (Elasticsearch) | Community |
| 25 | Nettoyer les fichiers vides ou les implémenter | Community, Gateway |
| 26 | Standardiser les commentaires (français ou anglais, pas les deux) | Tous |
| 27 | Propager `CancellationToken` dans tous les services | Identity |
| 28 | Remplir `LoginHistory.Country`/`City` et `RefreshToken.IpAddress`/`UserAgent` | Identity |
| 29 | Corriger `UpdateProfileAsync` pour supporter les partial updates (ne pas écraser avec null) | Identity |
| 30 | Remplacer `ISystemClock` déprécié par `TimeProvider` | Identity |

---

## 8. Résumé

Le projet **DotnetNiger** a une **excellente architecture et une bonne vision** avec une structure Clean Architecture bien pensée et une documentation solide. Cependant :

- **Identity (~75%)** est le seul service réellement fonctionnel, mais nécessite un travail important sur la sécurité (secrets, rate-limiting, hashing) et les tests
- **Community (~15%)** a un bon squelette d'entités et de DTOs mais ~82% des fichiers sont **vides** — le DbContext n'existe pas, rendant toute persistance impossible
- **Gateway (~25%)** a YARP fonctionnel mais AUCUN middleware, authentification, ou service n'est réellement câblé dans le pipeline
- **Les tests (~10%)** ne couvrent qu'une infime partie du code avec seulement 11 tests

Les **priorités immédiates** sont :
1. Sécuriser les secrets JWT
2. Fixer le bug de routing Gateway (`community-route` → `community-cluster`)
3. Implémenter le `CommunityDbContext` pour débloquer le développement Community
4. Câbler les middlewares du Gateway
5. Configurer CORS sur tous les services
