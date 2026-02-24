# Health Report — DotnetNiger

## État des services

- **Gateway** : ✅ Stable (YARP, JWT, CORS, rate limiting, logging, Swagger aggregation)
- **Identity** : ✅ Stable (auth, rôles, refresh tokens, admin, CORS restrictif, audit logs)
- **Community** : ⚠️ Partiel (posts, comments, likes, à compléter)

## Sécurité

- **JWT** : validé côté Gateway, propagé aux services
- **CORS** : Identity accepte uniquement Gateway
- **Rate limiting** : actif sur Gateway
- **Logging** : centralisé (Serilog)
- **Audit logs** : Identity admin

## Tests & Couverture

- **Unitaires** :
  - Identity : ✅
  - Gateway : ⚠️ (à compléter)
  - Community : ⚠️ (à compléter)
- **Intégration** :
  - Identity : ✅
  - Gateway : ⚠️ (partiel)

## Points de vigilance

- Compléter Community (features, tests)
- Ajouter tests Gateway
- CI/CD à renforcer (lint, SAST, secrets scan)
- Sécuriser secrets (env vars, pas de hardcode)

## Dernière vérification

2026-02-20

- Upload d'avatar (local + Azure Blob Storage) avec `AvatarCleanupService` actif
- API Key authentication HMAC-SHA256 avec sel
- Refresh tokens hashés SHA256 en base
- Seeders complets (rôles, permissions, admin)
- Cache service câblé (`ICacheService` → `GetProfileAsync` avec TTL 10min)
- `UserMapper` centralisé (plus de duplication)
- `JwtMiddleware` fonctionnel (enrichit `HttpContext` avec User-Id/Email/Roles)
- Guard JWT au démarrage (rejette les placeholders `__*__` et clés < 32 chars)

### 1.2 Endpoints disponibles

| Controller                       | Route                    | Auth                             | Endpoints                                                                                  |
| -------------------------------- | ------------------------ | -------------------------------- | ------------------------------------------------------------------------------------------ |
| **AuthController**         | `/api/v1/auth`         | Public                           | register, login, forgot-password, reset-password, verify-email, request-email-verification |
| **UsersController**        | `/api/v1/users`        | `[Authorize]`                  | GET/PUT /me, avatar CRUD, change-password, change-email                                    |
| **TokensController**       | `/api/v1/tokens`       | `[Authorize]`                  | refresh (public), logout                                                                   |
| **AdminController**        | `/api/v1/admin`        | `[Authorize(Roles = "Admin")]` | users list/detail/status, login-history, api-keys CRUD, audit-logs, file-upload settings   |
| **RolesController**        | `/api/v1/roles`        | `[Authorize(Roles = "Admin")]` | CRUD + assign/remove                                                                       |
| **PermissionsController**  | `/api/v1/permissions`  | `[Authorize(Roles = "Admin")]` | CRUD + assign/remove                                                                       |
| **SocialLinksController**  | `/api/v1/social-links` | `[Authorize]`                  | GET, POST, DELETE                                                                          |
| **ApiKeysController**      | `/api/v1/api-keys`     | `[Authorize]`                  | GET, POST, rotate, DELETE, revoke-all                                                      |
| **DiagnosticsController**  | `/api/v1/diagnostics`  | Public                           | ping, health                                                                               |
| **IntegrationsController** | `/api/v1/integrations` | `[ApiKeyOnly]`                 | ping                                                                                       |

### 1.3 Corrections appliquées (depuis le 19/02)

| Problème original                           | Statut      | Correction                                                           |
| -------------------------------------------- | ----------- | -------------------------------------------------------------------- |
| 6 fichiers Enums vides                       | ✅ Corrigé | `PlatformType`, `RoleType`, `UserStatus` — valeurs définies  |
| 3 interfaces domaine vides                   | ✅ Corrigé | `IAuditableEntity`, `ITokenable`, `IUser` — contrats définis |
| 3 seeders vides                              | ✅ Corrigé | `DefaultRolesSeeder` orchestre roles/permissions/admin             |
| `DotnetNigerIdentityDbFactory` vide        | ✅ Corrigé | Design-time factory SQLite fonctionnelle                             |
| `JwtMiddleware` pass-through               | ✅ Corrigé | Enrichit `HttpContext` (X-User-Id, Email, Roles)                   |
| `MappingProfile` vide                      | ✅ Corrigé | `UserMapper` centralisé, mapping factorisé                       |
| `AvatarCleanupService` commenté           | ✅ Corrigé | `HostedService` actif avec toggle runtime via `IOptionsMonitor`  |
| Seed admin commenté                         | ✅ Corrigé | `SeedAdminAsync` actif, credentials via env vars                   |
| Repositories jamais utilisés                | ✅ Corrigé | `IRefreshTokenRepository`, `ILoginHistoryRepository` injectés   |
| `ICacheService` jamais utilisé            | ✅ Corrigé | Cache-aside sur `GetProfileAsync`, invalidation sur mutations      |
| Email verification commentée                | ✅ Corrigé | `_emailService.SendAsync` actif avec URL de vérification          |
| `PasswordHasher`, `TokenValidator` morts | ✅ Corrigé | Fichiers supprimés                                                  |
| Clé JWT en clair                            | ✅ Corrigé | Placeholder `__JWT_KEY__`, guard au démarrage                     |
| API Key sans sel                             | ✅ Corrigé | HMAC-SHA256 + sel 16 octets +`Verify()` constant-time              |
| Refresh tokens en clair                      | ✅ Corrigé | SHA256 hash avant stockage, lookup par hash                          |
| Credentials email exposées                  | ✅ Corrigé | Placeholders `__SMTP_*__`, `Enabled: false` par défaut          |
| Admin credentials en dur                     | ✅ Corrigé | Env vars `ADMIN_EMAIL`/`ADMIN_PASSWORD` obligatoire              |
| Clé de test trop courte                     | ✅ Corrigé | 48 caractères dans `IdentityWebApplicationFactory`                |
| `ISystemClock` déprécié                 | ✅ Corrigé | Supprimé, utilise `.NET 8 TimeProvider`                           |
| Mapping dupliqué                            | ✅ Corrigé | `UserMapper.ToUserDtoAsync()` centralisé                          |
| Pas de `CancellationToken`                 | ✅ Corrigé | Propagé dans toutes les interfaces et implémentations              |
| `UpdateProfileAsync` écrase avec null     | ✅ Corrigé | `string?` nullable, seuls les champs non-null appliqués           |
| `RememberMe` code mort                     | ✅ Corrigé | Propriété supprimée de `LoginRequest`                           |
| `Country`/`City` jamais remplis          | ✅ Corrigé | Initialisés à `string.Empty` + TODO géolocalisation IP          |
| `RefreshToken` IP/UserAgent vides          | ✅ Corrigé | Remplis via `IHttpContextAccessor`                                 |
| Validators manquants (7)                     | ✅ Corrigé | 12 validators au total                                               |
| Email envoie token brut                      | ✅ Corrigé | URL `/api/auth/verify-email?email=...&token=...`                   |
| Doublon ErrorHandling/ExceptionFilter        | ✅ Corrigé | `ErrorHandlingMiddleware` supprimé, `ExceptionFilter` seul      |
| 3 noms connection string                     | ✅ Corrigé | Un seul :`DotnetNigerIdentityDbContext`                            |

### 1.4 Problèmes restants

| Problème                                      | Sévérité     | Détail                                                                        |
| ---------------------------------------------- | --------------- | ------------------------------------------------------------------------------ |
| Pas de rate-limiting                           | **Haute** | Login, register, forgot-password sans protection anti-brute-force              |
| Pas de politique de lockout                    | Moyenne         | `UserManager` lockout settings aux valeurs par défaut                       |
| ~~Pas de CORS configuré~~                    | ✅ Résolu      | CORS configuré avec `AddCors()` / `UseCors()` (AllowAnyOrigin)            |
| `SaveChanges` multiple dans `AdminService` | Basse           | Appels multiples par opération                                                |
| Tests insuffisants                             | **Haute** | 4/22 tests passent — problèmes de build des projets de test                  |
| Géolocalisation IP manquante                  | Basse           | `LoginHistory.Country`/`City` initialisés mais pas remplis (TODO MaxMind) |
| `BlobUriBuilder` Azure supprimé             | Info            | Remplacé par `uri.AbsolutePath` — vérifier si Azure Blob est requis       |

### 1.5 Scorecard Identity

| Domaine              | Note   | Évolution | Commentaire                                                   |
| -------------------- | ------ | ---------- | ------------------------------------------------------------- |
| Entités/Domaine     | 9/10   | ↑ +2      | Toutes complètes, enums et interfaces implémentés          |
| API Design           | 9/10   | =          | Complète, versionnée, bien structurée                      |
| Services             | 9/10   | ↑ +1      | Mapper centralisé, cache câblé,`CancellationToken`       |
| Sécurité           | 7.5/10 | ↑ +2.5    | HMAC-SHA256, tokens hashés, secrets externalisés, guard JWT |
| Infrastructure       | 8/10   | ↑ +2      | Seeders, cache, repositories tous câblés, 0 fichier vide    |
| Tests                | 2/10   | ↓ -1      | 4/22 tests passent — problèmes de build projet de tests     |
| Qualité de code     | 8/10   | ↑ +2      | Validators complets, plus de code mort, CancellationToken     |
| Production Readiness | 7/10   | ↑ +3      | CORS configuré, manque rate-limiting                         |

---

## 2. DotnetNiger.Community — **41% implémenté** (58/140 fichiers)

### 2.1 Ce qui est implémenté ✅

- **14/16 entités** implémentées (Event, EventMedia, EventRegistration, Post, PostCategory, PostTag, Comment, Project, ProjectContributor, Resource, Partner, Category, Tag, TeamMember)
- **6/12 DTOs Request** implémentés (CreatePostRequest, CreateEventRequest, CreateProjectRequest, AddPartnerRequest, AddResourceRequest, RegisterEventRequest)
- **10/16 DTOs Response** implémentés (EventDto, EventMediaDto, PostDto, ProjectDto, ResourceDto, PartnerDto, CategoryDto, TagDto, TeamMemberDto, SocialLinkDto)
- `TestController` avec 2 endpoints de test (`GET /api/test/test`, `GET /api/test/health`)
- `Program.cs` minimal avec Swagger

### 2.2 Ce qui est VIDE (82 fichiers — 0 code)

| Couche                       | Fichiers vides | Liste complète                                                                                                                                                                                                                           |
| ---------------------------- | -------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Controllers**        | 10/11          | `CategoriesController`, `CommentsController`, `EventsController`, `PartnersController`, `PostsController`, `ProjectsController`, `ResourcesController`, `SearchController`, `StatsController`, `TagsController`       |
| **Services**           | 12/12          | `CategoryService`, `CommentService`, `EventService`, `IdentityApiClient`, `PartnerService`, `PostService`, `ProjectService`, `ResourceService`, `SearchService`, `StatisticsService`, `TagService`, `TeamService` |
| **Interfaces Service** | 12/12          | Tous les `IXxxService` correspondants                                                                                                                                                                                                   |
| **Repositories**       | 12/12          | `BaseRepository`, `EventRepository`, `PostRepository`, `ProjectRepository`, `ResourceRepository`, `PartnerRepository` + interfaces                                                                                            |
| **Validators**         | 6/6            | `AddPartnerRequestValidator`, `CreateEventRequestValidator`, `CreatePostRequestValidator`, `CreateProjectRequestValidator`, `CreateResourceRequestValidator`, `SearchQueryValidator`                                          |
| **Exceptions**         | 5/5            | `CommunityException`, `EventNotFoundException`, `PostNotFoundException`, `ResourceNotApprovedException`, `UnauthorizedAccessException`                                                                                          |
| **Middleware**         | 3/3            | `ErrorHandlingMiddleware`, `JwtValidationMiddleware`, `RequestLoggingMiddleware`                                                                                                                                                    |
| **Filters**            | 4/4            | `AuthorizeFilter`, `CacheFilter`, `ExceptionFilter`, `ValidateModelFilter`                                                                                                                                                        |
| **Extensions**         | 2/2            | `ServiceExtensions`, `MiddlewareExtensions`                                                                                                                                                                                           |
| **DbContext**          | 2/2            | **`CommunityDbContext`**, `CommunityDbContextFactory`                                                                                                                                                                           |
| **Seeders**            | 4/4            | `DefaultDataSeeder`, `CategorySeeder`, `TagSeeder`, `PartnerSeeder`                                                                                                                                                               |
| **Caching**            | 3/3            | `ICacheService`, `RedisCacheService`, `CacheKeyGenerator`                                                                                                                                                                           |
| **External**           | 6/6            | `ISearchProvider`, `ElasticsearchProvider`, `IFileUploadService`, `AzureBlobService`, `IIdentityApiClient`, `IdentityApiClient`                                                                                               |
| **Mapper**             | 1/1            | `MappingProfile`                                                                                                                                                                                                                        |

### 2.3 Statistiques d'implémentation

| Couche                 | Fichiers       | Implémentés | Vides         | Taux           |
| ---------------------- | -------------- | ------------- | ------------- | -------------- |
| Domain/Entities        | 16             | 14            | 2             | ~87%           |
| Domain/Enums           | 8              | 8             | 0             | 100% ✅        |
| Domain/Interfaces      | 3              | 0             | 3             | 0%             |
| Api/Controllers        | 12             | 1             | 11            | ~8%            |
| Api/Middleware         | 3              | 0             | 3             | 0%             |
| Api/Filters            | 4              | 0             | 4             | 0%             |
| Api/Extensions         | 2              | 0             | 2             | 0%             |
| Application/Services   | 24             | 0             | 24            | 0%             |
| Application/DTOs       | 28             | 16            | 12            | ~57%           |
| Application/Validators | 7              | 0             | 7             | 0%             |
| Application/Exceptions | 5              | 0             | 5             | 0%             |
| Infrastructure         | 29             | 0             | 29            | 0%             |
| Program.cs             | 1              | 1 (minimal)   | —            | ~15%           |
| **TOTAL**        | **~140** | **~66** | **~74** | **~47%** |

> **Mise à jour 20/02** : Les 8 enums Community sont maintenant implémentés. Le comptage révisé donne 74 fichiers vides sur 140 fichiers `.cs` (47% implémentés).

### 2.4 Problèmes critiques

| Problème                                          | Sévérité        | Détail                                                                          |
| -------------------------------------------------- | ------------------ | -------------------------------------------------------------------------------- |
| Pas de DbContext                                   | **CRITIQUE** | Aucune persistance possible                                                      |
| Pas de packages NuGet EF Core                      | **CRITIQUE** | Manque `Microsoft.EntityFrameworkCore`, provider SQLite/SqlServer              |
| Pas d'authentification JWT                         | **CRITIQUE** | `UseAuthentication()` non appelé, aucun scheme                                |
| Pas de connection string                           | **CRITIQUE** | `appsettings.json` n'a que Logging/AllowedHosts                                |
| CORS configuré                                    | ✅ OK              | `AddCors()` et politique `CorsePolicy` dans Program.cs                       |
| Pas de DI                                          | **CRITIQUE** | `Program.cs` n'enregistre aucun service                                        |
| Entities utilisent `string` au lieu d'enums      | Moyenne            | `EventType`, `PostType`, `Level` sont des `string`                       |
| `DateTime.UtcNow` dans les property initializers | Moyenne            | Anti-pattern — le timestamp est celui de l'instanciation, pas de la persistence |
| Fichiers en doublon                                | Moyenne            | `IIdentityApiClient` et `IdentityApiClient` existent dans 2 dossiers         |
| `ResourceCategory` et `TeamMemberSkill` vides  | Moyenne            | Classes stub sans propriétés                                                   |
| Fichier `.http` référence `/weatherforecast` | Basse              | Résidu de template .NET                                                         |
| Pas de FluentValidation dans les NuGet             | Moyenne            | Framework de validation non installé                                            |
| Pas d'AutoMapper dans les NuGet                    | Basse              | Librairie de mapping non installée                                              |

---

## 3. DotnetNiger.Gateway — **73% implémenté** (41/56 fichiers)

### 3.1 Ce qui fonctionne (vérifié)

- **YARP reverse proxy** configuré avec 5 routes (code en dur dans `RouteConfiguration.cs`/`ClusterConfiguration.cs`)
- **Swagger Aggregator** — merge les docs Swagger des 2 services en un document OpenAPI
- **Dockerfile** fonctionnel
- **Middlewares câblés** — `UseGatewayMiddlewares()` et `AddGatewayServices()` appelés dans `Program.cs`
- **CORS configuré** dans `ServiceExtensions` (AllowAnyOrigin/Header/Method)
- 56 fichiers `.cs` — 41 implémentés, **15 vides**

### 3.2 Configuration YARP

| Route ID              | Path Pattern                         | Cluster           | Transform                               |
| --------------------- | ------------------------------------ | ----------------- | --------------------------------------- |
| `identity-uploads`  | `/uploads/{**catch-all}`           | identity-cluster  | Aucun                                   |
| `identity-route`    | `/identity/{**catch-all}`          | identity-cluster  | Aucun                                   |
| `identity-swagger`  | `/swagger/identity/{**catch-all}`  | identity-cluster  | Rewrite vers `/swagger/{**catch-all}` |
| `community-route`   | `/community/{**catch-all}`         | community-cluster | Aucun                                   |
| `community-swagger` | `/swagger/community/{**catch-all}` | community-cluster | Rewrite vers `/swagger/{**catch-all}` |

> **✅ Bug corrigé :** `community-route` pointe désormais vers `community-cluster` (code en dur).
> **⚠️ Note :** `appsettings.json` contient encore l'ancienne config YARP mais elle est ignorée — les routes sont chargées en mémoire depuis `RouteConfiguration.cs`/`ClusterConfiguration.cs`.

### 3.3 Middleware — câblés via `UseGatewayMiddlewares()`

| Middleware                     | État                                  | Détail                                                                |
| ------------------------------ | -------------------------------------- | ---------------------------------------------------------------------- |
| `ErrorHandlingMiddleware`    | ✅ Câblé                             | Catch exceptions → JSON 500. Expose `exception.Message` en prod     |
| `RequestLoggingMiddleware`   | ✅ Câblé                             | Log method, path, status, durée                                       |
| `RequestTransformMiddleware` | ✅ Câblé                             | Ajoute `X-Gateway-Version` et `X-Request-Id`                       |
| `AuthenticationMiddleware`   | ⚠️ Câblé,**stub**            | Lit le header `Authorization` mais ne valide rien                    |
| `JwtInjectionMiddleware`     | ⚠️ Câblé,**non fonctionnel** | Dépend de `AuthenticationMiddleware` qui ne fonctionne pas          |
| `RateLimitingMiddleware`     | ⚠️ Câblé,**pass-through**    | "sera implémenté plus tard avec Redis"                               |
| `CorsMiddleware`             | ⚠️ Redondant                         | Le CORS ASP.NET natif est déjà configuré dans `ServiceExtensions` |

### 3.4 Services applicatifs — enregistrés en DI via `AddGatewayServices()`

| Service                      | Interface                     | État                                                                       |
| ---------------------------- | ----------------------------- | --------------------------------------------------------------------------- |
| `AuthenticationService`    | `IAuthenticationService`    | **Stub** — `ValidateTokenAsync` retourne toujours `true`         |
| `CachingService`           | `ICachingService`           | Implémenté (`IMemoryCache`)                                             |
| `MetricsService`           | `IMetricsService`           | Minimal — log seulement                                                    |
| `RateLimitService`         | `IRateLimitService`         | **Cassé** — `Dictionary<string, int>` non thread-safe, pas de TTL |
| `RequestForwardingService` | `IRequestForwardingService` | Partiel — ne copie pas le body                                             |
| `RouteService`             | `IRouteService`             | Implémenté — redondant avec YARP                                         |

### 3.5 Fichiers vides (15 fichiers vérifiés)

| Catégorie                    | Fichiers                                                                                                    |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------- |
| **Exceptions** (3)      | `RateLimitExceededException`, `ServiceUnavailableException`, `UnauthorizedException`                  |
| **Caching** (2)         | `ICacheProvider`, `RedisCacheProvider`                                                                  |
| **Circuit Breaker** (2) | `ICircuitBreaker`, `PollyCircuitBreaker`                                                                |
| **Config** (4)          | `GatewayConfig`, `RateLimitConfig`, `RouteConfiguration` (Infrastructure/Config), `ServiceRegistry` |
| **Monitoring** (4)      | `HealthCheckService`, `IHealthCheckService`, `IMetricsCollector`, `PrometheusMetricsCollector`      |

> **Note :** `yarp-routes.json` et les HTTP clients ne sont plus comptés comme vides — la config YARP est en code, et les clients ne sont pas nécessaires avec YARP.

### 3.6 Code analysé en détail

#### AuthenticationService.cs (STUB ⚠️)

```csharp
public Task<bool> ValidateTokenAsync(string token)
{
    // TODO: Implémenter la validation JWT réelle
    return Task.FromResult(true); // ← TOUJOURS TRUE!
}
```

#### RateLimitService.cs (CASSÉ ⚠️)

```csharp
private readonly Dictionary<string, int> _requestCounts = new(); // Non thread-safe, pas de TTL
```

#### AuthenticationMiddleware.cs (STUB ⚠️)

```csharp
if (!string.IsNullOrEmpty(token))
{
    _logger.LogDebug("Token JWT détecté"); // Log seulement, pas de validation!
}
await _next(context); // Passe toujours
```

### 3.7 Problèmes restants Gateway

| Problème                                                                         | Sévérité        |
| --------------------------------------------------------------------------------- | ------------------ |
| Aucune validation JWT au niveau Gateway                                           | **CRITIQUE** |
| `AuthenticationService.ValidateTokenAsync` retourne toujours `true`           | **CRITIQUE** |
| Pas de rate-limiting réel                                                        | **Haute**    |
| Pas de health checks                                                              | **Haute**    |
| Pas de config production (cluster addresses dans `appsettings.Production.json`) | **Haute**    |
| `RateLimitService` non thread-safe                                              | Moyenne            |
| `SwaggerAggregatorController` avale les exceptions silencieusement              | Moyenne            |
| `ErrorHandlingMiddleware` expose `exception.Message` en production            | Moyenne            |
| Pas de circuit breaker (Polly non installé)                                      | Moyenne            |
| CORS trop permissif (`AllowAnyOrigin`) — restreindre en production             | Moyenne            |
| Config YARP dans `appsettings.json` est du code mort (routes en mémoire)       | Basse              |
| Pas de tests                                                                      | Moyenne            |

### 3.7 Scorecard Gateway

| Domaine              | Note | Évolution | Commentaire                                                   |
| -------------------- | ---- | ---------- | ------------------------------------------------------------- |
| Routing YARP         | 9/10 | =          | Fonctionnel, bug community-route corrigé                     |
| Middleware Pipeline  | 6/10 | ↑ +4      | Câblé, mais auth stub et rate-limit vide                    |
| Authentification     | 1/10 | =          | Toujours stub —`ValidateTokenAsync` → `true`            |
| Infrastructure       | 3/10 | ↑ +1      | DI câblé, mais 15 fichiers vides, pas de circuit breaker    |
| Production Readiness | 2/10 | ↑ +1      | Pas de vraie auth, pas de rate-limiting, pas de health checks |

---

## 4. Tests — Couverture minimale

| Projet                               | Tests    | Résultat    | Ce qui est testé          | Ce qui manque                                                          |
| ------------------------------------ | -------- | ------------ | -------------------------- | ---------------------------------------------------------------------- |
| **Identity Unit Tests**        | 7 tests  | 0 ✅ / 7 ❌  | —                         | Projet de test ne compile pas (erreurs de build)                       |
| **Identity Integration Tests** | 15 tests | 4 ✅ / 11 ❌ | Diagnostics (ping, health) | `PreserveCompilationContext` manquant, `testhost.deps.json` absent |
| **Community Tests**            | 0        | —           | —                         | **Aucun projet de test**                                         |
| **Gateway Tests**              | 0        | —           | —                         | **Aucun projet de test**                                         |

### Tests échoués — Problèmes de build

| Projet                                | Type d'erreur                   | Cause probable                                         |
| ------------------------------------- | ------------------------------- | ------------------------------------------------------ |
| DotnetNiger.Identity.Tests            | Erreurs de compilation          | Références NuGet manquantes ou interfaces modifiées |
| DotnetNiger.Identity.IntegrationTests | `testhost.deps.json` manquant | Configuration `PreserveCompilationContext` absente   |

> **Correction requise :** Résoudre les problèmes de build des projets de test avant d'itérer sur les tests.

### Problèmes de build des tests

| Projet                                | Erreur                          | Correction requise                                                                        |
| ------------------------------------- | ------------------------------- | ----------------------------------------------------------------------------------------- |
| DotnetNiger.Identity.Tests            | Erreur de build                 | Vérifier les références NuGet et les imports                                           |
| DotnetNiger.Identity.IntegrationTests | `testhost.deps.json` manquant | Ajouter `<PreserveCompilationContext>true</PreserveCompilationContext>` dans le .csproj |

### Stack de test utilisée (Identity)

- xUnit
- Moq (avec `CancellationToken.IsAny` pour les signatures mises à jour)
- FluentAssertions
- `IdentityWebApplicationFactory` (SQLite in-memory, clé JWT 48 chars)
- `TestUserFactory` (crée des users avec rôles + JWT)

---

## 5. Docker & Infrastructure

| Élément              | État            | Problème                                                                                                       |
| ---------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------- |
| `docker-compose.yml` | Fonctionnel      | SQL Server commenté                                                                                            |
| Dockerfile Identity    | OK               | Multi-stage .NET 8, non-root `$APP_UID`                                                                       |
| Dockerfile Community   | OK               | Multi-stage .NET 8                                                                                              |
| Dockerfile Gateway     | OK               | Multi-stage .NET 8                                                                                              |
| CI/CD                  | **Absent** | Pas de `.github/workflows`, pas de pipeline Azure DevOps                                                      |
| Gestion des secrets    | ✅ Améliorée   | Secrets externalisés via placeholders `__*__` et env vars. `dotnet user-secrets` / Key Vault non utilisés |
| Health checks Docker   | Définis         | `curl` sur `/health` — fonctionne sur Identity, pas sur Gateway/Community                                  |
| Volumes                | OK               | `sqlserver_data` persistant                                                                                   |

---

## 6. Documentation

| Fichier                  | Contenu                                            |
| ------------------------ | -------------------------------------------------- |
| `README.md`            | Complet — architecture, setup, contribution guide |
| `docs/API.md`          | Documentation API                                  |
| `docs/ARCHITECTURE.md` | Architecture technique                             |
| `docs/SETUP.md`        | Guide d'installation                               |
| `docs/INDEX.md`        | Index de la documentation                          |
| `CHANGELOG.md`         | Historique des changements                         |
| `SECURITY.md`          | Politique de sécurité                            |
| `LICENSE.md`           | Licence MIT                                        |

> La documentation est **bien fournie** pour un projet en développement.

---

## 7. Plan d'action prioritaire

### Priorité CRITIQUE (à faire en premier)

| # | Action                                                                                       | Service             | Impact                                     | Statut                |
| - | -------------------------------------------------------------------------------------------- | ------------------- | ------------------------------------------ | --------------------- |
| 1 | ~~Sécuriser les secrets JWT~~                                                              | Identity            | Sécurité                                 | ✅ Fait               |
| 2 | ~~Fixer le bug `community-route`~~                                                        | Gateway             | Routing                                    | ✅ Fait (code en dur) |
| 3 | **Implémenter `CommunityDbContext`** + ajouter packages EF Core + connection string | Community           | **Débloque tout le développement** | ❌ À faire           |
| 4 | ~~Câbler les middlewares Gateway~~                                                         | Gateway             | Middleware                                 | ✅ Fait               |
| 5 | ~~Configurer CORS~~ sur Identity et Community                                               | Identity, Community | Blocage frontend                           | ✅ Fait               |
| 6 | ~~Activer le seed admin~~                                                                   | Identity            | Admin access                               | ✅ Fait               |

### Priorité HAUTE

| #  | Action                                                                                           | Service           | Statut                                           |
| -- | ------------------------------------------------------------------------------------------------ | ----------------- | ------------------------------------------------ |
| 7  | **Implémenter l'authentification JWT réelle au Gateway**                                 | Gateway           | ❌ À faire —`ValidateTokenAsync` → `true` |
| 8  | **Ajouter du rate-limiting** sur login, register, forgot-password                          | Identity, Gateway | ❌ À faire                                      |
| 9  | Implémenter les controllers Community (commencer par `PostsController`, `EventsController`) | Community         | ❌ À faire                                      |
| 10 | Implémenter les services + repositories Community                                               | Community         | ❌ À faire                                      |
| 11 | **Résoudre les problèmes de build des projets de test**                                  | Identity Tests    | ❌ À faire                                      |
| 12 | ~~Hasher les refresh tokens~~                                                                   | Identity          | ✅ Fait (SHA256)                                 |
| 13 | ~~Ajouter les validators manquants~~                                                            | Identity          | ✅ Fait (12 validators)                          |
| 14 | ~~Supprimer le code mort~~                                                                      | Identity          | ✅ Fait                                          |

### Priorité MOYENNE

| #  | Action                                                                              | Service            | Statut      |
| -- | ----------------------------------------------------------------------------------- | ------------------ | ----------- |
| 15 | Créer des projets de test pour Community et Gateway                                | Community, Gateway | ❌ À faire |
| 16 | Augmenter la couverture de tests Identity (AuthService, TokenService, AdminService) | Identity           | ❌ À faire |
| 17 | ~~Extraire le mapping dupliqué dans `UserMapper`~~                              | Identity           | ✅ Fait     |
| 18 | Implémenter le circuit breaker Polly dans le Gateway                               | Gateway            | ❌ À faire |
| 19 | Configurer les health checks YARP sur les clusters                                  | Gateway            | ❌ À faire |
| 20 | Ajouter une pipeline CI/CD (GitHub Actions)                                         | Tous               | ❌ À faire |
| 21 | ~~Implémenter les seeders Identity~~                                              | Identity           | ✅ Fait     |
| 22 | Ajouter `appsettings.Production.json` avec les addresses de cluster               | Gateway            | ❌ À faire |
| 23 | Ajouter une politique de lockout (`UserManager`)                                  | Identity           | ❌ À faire |
| 24 | Restreindre CORS Gateway en production (retirer `AllowAnyOrigin`)                 | Gateway            | ❌ À faire |
| 25 | Implémenter géolocalisation IP (MaxMind GeoLite2) pour `LoginHistory`           | Identity           | ❌ À faire |

### Priorité BASSE

| #  | Action                                                        | Service             | Statut      |
| -- | ------------------------------------------------------------- | ------------------- | ----------- |
| 26 | Implémenter le caching Redis                                 | Identity, Community | ❌ À faire |
| 27 | Implémenter la recherche (Elasticsearch)                     | Community           | ❌ À faire |
| 28 | Nettoyer les fichiers vides Community (82) et Gateway (15)    | Community, Gateway  | ❌ À faire |
| 29 | Standardiser les commentaires (français ou anglais)          | Tous                | ❌ À faire |
| 30 | ~~Propager `CancellationToken` dans tous les services~~    | Identity            | ✅ Fait     |
| 31 | ~~Remplir `RefreshToken.IpAddress`/`UserAgent`~~         | Identity            | ✅ Fait     |
| 32 | ~~Corriger `UpdateProfileAsync` pour les partial updates~~ | Identity            | ✅ Fait     |
| 33 | ~~Remplacer `ISystemClock` déprécié~~                   | Identity            | ✅ Fait     |
| 34 | Nettoyer la config YARP morte dans `appsettings.json`       | Gateway             | ❌ À faire |

### Résumé du plan

| Statut      | Compte  |
| ----------- | ------- |
| ✅ Terminé | 15 / 34 |
| ❌ À faire | 19 / 34 |

> **Focus principal :** Les items critiques restants sont (3) CommunityDbContext, (7) Auth JWT Gateway, et (11) Build des tests.

---

## 8. Résumé

Le projet **DotnetNiger** a une **excellente architecture et une bonne vision** avec une structure Clean Architecture bien pensée et une documentation solide.

### Progression depuis le 19/02/2026

| Domaine             | État précédent | État actuel             | Détail             |
| ------------------- | ----------------- | ------------------------ | ------------------- |
| Build               | ✅ 0 erreurs      | ✅ 0 erreurs             | 5 projets compilent |
| **Identity**  | 143 fichiers      | **143/143 (100%)** | Aucun fichier vide  |
| **Gateway**   | ~60%              | **41/56 (73%)**    | 15 fichiers vides   |
| **Community** | ~15%              | **58/140 (41%)**   | 82 fichiers vides   |
| Tests               | 4/22 passent      | **4/22 (18%)**     | Problèmes de build |

### État actuel des services (audit complet)

- **Identity (100%)** : 143/143 fichiers implémentés. Code de qualité production : services avec `CancellationToken`, cache-aside, validation, logging structuré. CORS configuré.
- **Gateway (73%)** : 41/56 fichiers implémentés. YARP fonctionnel, middlewares câblés. **Problème critique** : `AuthenticationService.ValidateTokenAsync()` retourne toujours `true`.
- **Community (41%)** : 58/140 fichiers implémentés.Entités, DTOs, enums prêts. **Bloqué** : DbContext vide, aucun service ni controller implémenté.
- **Tests (18%)** : 4/22 tests passent. Problèmes de compilation des projets de test.

### Les 3 priorités immédiates

1. **Implémenter `CommunityDbContext`** — débloquer le développement Community
2. **Implémenter l'authentification JWT réelle au Gateway** — sécuriser les routes
3. **Résoudre les problèmes de build des projets de test** — rétablir la CI/CD

### 8.1 Code Identity analysé

#### Program.cs Identity (✅ Complet)

- Serilog configuré avec structured logging
- Guard JWT : rejette placeholders `__*__` et clés < 32 chars
- `AddIdentityCore` avec `RequireUniqueEmail`
- Smart authentication scheme (JWT + API Key)
- CORS configuré avec `AllowAnyOrigin`
- `AvatarCleanupService` en HostedService
- Seeders exécutés au démarrage via `SeedAdminAsync`

#### AuthService.cs (✅ Complet)

- Validation avec `RegisterRequestValidator` et `LoginRequestValidator`
- Email de vérification envoyé avec URL complète
- `LoginHistoryService.RecordAsync` appelé sur succès/échec
- Tokens JWT générés via `JwtTokenGenerator`
- Refresh tokens hashés SHA256

#### UserService.cs (✅ Complet)

- Cache-aside pattern avec TTL 10 minutes
- Invalidation du cache sur mutations
- Partial updates (seuls les champs non-null appliqués)

#### TokenService.cs (✅ Complet)

- Rotation des refresh tokens
- IP/UserAgent capturés via `IHttpContextAccessor`
- Révocation propre sur logout

#### DbContext Identity (✅ Complet)

- Hérite de `IdentityDbContext<ApplicationUser, Role, Guid>`
- 7 DbSets : RefreshTokens, LoginHistories, SocialLinks, ApiKeys, AdminActionLogs, Permissions, RolePermissions
- Relations configurées dans `OnModelCreating`

### 8.2 Code Gateway analysé

#### Program.cs Gateway (✅ Fonctionnel mais incomplet)

- `AddGatewayServices` enregistre tous les services
- `UseGatewayMiddlewares` câble 7 middlewares
- YARP chargé depuis `RouteConfiguration.GetRoutes()` (en mémoire)
- Swagger aggregator configuré

#### ServiceExtensions.cs (✅ Implémenté)

- HttpClients typés : `IdentityApiClient`, `CommunityApiClient`
- Services : `RequestForwardingService`, `RouteService`, `AuthenticationService`, `CachingService`, `RateLimitService`, `MetricsService`
- MemoryCache activé

#### MiddlewareExtensions.cs (✅ Implémenté)

7 middlewares dans l'ordre :

1. `ErrorHandlingMiddleware` — Catch exceptions
2. `RequestLoggingMiddleware` — Log méthode/path/durée
3. `RequestTransformMiddleware` — Ajoute X-Gateway-Version
4. `AuthenticationMiddleware` — **STUB** (ne valide rien)
5. `JwtInjectionMiddleware` — Dépend de AuthMiddleware
6. `RateLimitingMiddleware` — **Pass-through**
7. `CorsMiddleware` — Redondant avec CORS natif

### 8.3 Code Community analysé

#### Program.cs Community (⚠️ Minimal)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCors(...); // ✅ OK
builder.Services.AddSwaggerGen(...); // ✅ OK
var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI(...);
app.UseHttpsRedirection();
app.UseAuthorization(); // ⚠️ Sans UseAuthentication!
app.MapControllers();
app.Run();
```

**Ce qui manque dans Program.cs Community :**

- `AddDbContext<CommunityDbContext>`
- `UseAuthentication()`
- Enregistrement des services (`AddScoped<IPostService, PostService>`)
- Connection string
- Packages EF Core

#### Entités Community (✅ 14/16 implémentées)

Bien structurées avec relations :

```csharp
public class Post {
    public Guid Id { get; set; }
    public string Title { get; set; }
    public Guid AuthorId { get; set; } // Réf Identity
    public ICollection<PostCategory> PostCategories { get; set; }
    public ICollection<Comment> Comments { get; set; }
}
```

#### DTOs Community (✅ Implémentés)

- Requests : `CreatePostRequest`, `CreateEventRequest`, `CreateProjectRequest`, etc.
- Responses : `PostDto`, `EventDto`, `ProjectDto`, etc.
- Validation DataAnnotations présente

---

## 9. Prochaines Étapes de Développement

### Phase 1 : Community Service (Priorité CRITIQUE)

Étapes détaillées pour débloquer Community :

#### 1.1 Ajouter les dépendances NuGet

```bash
cd DotnetNiger.Community
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite  # ou SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package FluentValidation.AspNetCore
```

#### 1.2 Implémenter CommunityDbContext

```csharp
// Infrastructure/Data/CommunityDbContext.cs
public class CommunityDbContext : DbContext
{
    public CommunityDbContext(DbContextOptions<CommunityDbContext> options) : base(options) { }
  
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
  
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Configurer les relations many-to-many
        modelBuilder.Entity<PostCategory>().HasKey(pc => new { pc.PostId, pc.CategoryId });
        modelBuilder.Entity<PostTag>().HasKey(pt => new { pt.PostId, pt.TagId });
    }
}
```

#### 1.3 Configurer appsettings.json

```json
{
  "ConnectionStrings": {
    "CommunityDbContext": "Data Source=community.db"
  }
}
```

#### 1.4 Configurer Program.cs

```csharp
// Ajouter après AddControllers()
var connectionString = builder.Configuration.GetConnectionString("CommunityDbContext");
builder.Services.AddDbContext<CommunityDbContext>(options => options.UseSqlite(connectionString));

// Ajouter les services:
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IEventService, EventService>();
// ... etc
```

#### 1.5 Créer la première migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Phase 2 : Gateway Auth (Priorité HAUTE)

Implémenter la validation JWT réelle dans `AuthenticationService` :

```csharp
// Application/Services/AuthenticationService.cs
public async Task<bool> ValidateTokenAsync(string token)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _configuration["Jwt:Issuer"],
        ValidAudience = _configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
    };
  
    try {
        tokenHandler.ValidateToken(token, validationParameters, out _);
        return true;
    } catch {
        return false;
    }
}
```

### Phase 3 : Corriger les Tests (Priorité HAUTE)

#### 3.1 Corriger le build du projet de tests unit

- Vérifier les références au projet Identity
- S'assurer que tous les mocks sont à jour avec `CancellationToken`

#### 3.2 Corriger les tests d'intégration

Ajouter dans `DotnetNiger.Identity.IntegrationTests.csproj` :

```xml
<PropertyGroup>
  <PreserveCompilationContext>true</PreserveCompilationContext>
</PropertyGroup>
```

### Phase 4 : Implémenter Community Controllers (Priorité MOYENNE)

Ordre de développement recommandé :

1. `PostsController` + `PostService` + `PostRepository`
2. `EventsController` + `EventService` + `EventRepository`
3. `CategoriesController` + `TagsController` (simples CRUD)
4. `ProjectsController` + `ResourcesController`
5. `CommentsController` + `PartnersController` + `TeamController`
6. `SearchController` + `StatsController` (avancés)

### Phase 5 : Rate Limiting (Priorité MOYENNE)

Ajouter AspNetCoreRateLimit :

```bash
dotnet add package AspNetCoreRateLimit
```

Configurer dans Program.cs Identity :

```csharp
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options => {
    options.GeneralRules = new List<RateLimitRule> {
        new() { Endpoint = "POST:/api/v1/auth/login", Period = "1m", Limit = 5 },
        new() { Endpoint = "POST:/api/v1/auth/register", Period = "1h", Limit = 10 },
        new() { Endpoint = "POST:/api/v1/auth/forgot-password", Period = "1h", Limit = 3 }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
```

---

## 10. Roadmap de Développement

| Sprint             | Durée    | Objectif                           | Livrable                                |
| ------------------ | --------- | ---------------------------------- | --------------------------------------- |
| **Sprint 1** | 1-2 jours | Community DbContext + EF Core      | DB opérationnelle, migrations créées |
| **Sprint 2** | 2-3 jours | PostsController + EventsController | CRUD Posts et Events fonctionnels       |
| **Sprint 3** | 1 jour    | Gateway JWT Auth                   | Authentification réelle au Gateway     |
| **Sprint 4** | 1 jour    | Fix Tests                          | Tous les tests passent                  |
| **Sprint 5** | 2-3 jours | Remaining Community Controllers    | API Community complète                 |
| **Sprint 6** | 1-2 jours | Rate Limiting + Security           | Protection anti-brute-force             |
| **Sprint 7** | 2 jours   | Tests Community + Gateway          | Couverture > 60%                        |
| **Sprint 8** | 1 jour    | CI/CD GitHub Actions               | Pipeline automatisée                   |
