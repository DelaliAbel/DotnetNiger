# 🛠️ Stack Technique - DotnetNiger

**Dernière mise à jour:** 29 Janvier 2026  
**Version:** 1.0.0  
**.NET Version:** 8.0 LTS

---

## 🎯 Technologies Principales

### Architecture: MVC (Model-View-Controller)

**Pattern de développement** pour maintenir une séparation claire des responsabilités

---

## 🔐 Authentification & Sécurité

### ASP.NET Identity + JWT Bearer

| Technologie | Version | Usage |
|-------------|---------|-------|
| **ASP.NET Core Identity** | 8.0 | Gestion des utilisateurs et rôles |
| **JWT Bearer Authentication** | 8.0 | Tokens d'authentification |
| **HTTPS** | TLS 1.3 | Chiffrement des communications |
| **CORS** | Built-in | Contrôle des origines |

**Configuration JWT:**
```json
{
  "Jwt": {
    "Secret": "[SECRET_256_BITS]",
    "Issuer": "dotnetniger",
    "Audience": "dotnetniger-api",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## 🔧 Backend

### Framework & API

| Technologie | Version | Description |
|-------------|---------|-------------|
| **ASP.NET Core** | 8.0 LTS | Framework web principal |
| **ASP.NET Core MVC** | 8.0 | Pattern MVC pour les APIs |
| **Minimal APIs** | 8.0 | APIs légères (optionnel) |
| **C#** | 12.0 | Langage principal |

---

### Base de Données

| Technologie | Version | Usage |
|-------------|---------|-------|
| **SQL Server** | 2022+ | Base de données principale |
| **PostgreSQL** | 16+ | Alternative |
| **Entity Framework Core** | 8.0 | ORM (Object-Relational Mapper) |
| **EF Core Migrations** | 8.0 | Gestion du schéma BD |

**Exemple de connexion:**
```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Server=localhost;Database=IdentityDb;User Id=sa;Password=***",
    "CommunityConnection": "Server=localhost;Database=CommunityDb;User Id=sa;Password=***"
  }
}
```

---

### Hashing & Cryptographie

| Technologie | Usage |
|-------------|-------|
| **PBKDF2** | Hash des mots de passe (Identity built-in) |
| **BCrypt** | Alternative pour hash (optionnel) |
| **SHA256** | Signature JWT |
| **AES** | Chiffrement de données sensibles (optionnel) |

---

### Sécurité

| Technologie | Usage |
|-------------|-------|
| **HTTPS/TLS** | Obligatoire en production |
| **CORS** | Strict - Domaines autorisés uniquement |
| **Rate Limiting** | AspNetCore.RateLimiting |
| **Input Validation** | FluentValidation + DataAnnotations |
| **SQL Injection Protection** | EF Core paramétré |
| **XSS Protection** | Sanitization des inputs |

---

## 📚 Documentation

### API Documentation

| Technologie | Version | Usage |
|-------------|---------|-------|
| **Swagger (Swashbuckle)** | 6.5+ | Interface interactive API |
| **OpenAPI** | 3.0 | Spécification standard |

**Accessible à:**
- Development: `http://localhost:5000/swagger`
- Production: `https://api.dotnetniger.com/swagger` (optionnel)

---

## 📊 Logging

### Serilog

| Composant | Usage |
|-----------|-------|
| **Serilog** | Framework de logging structuré |
| **Serilog.AspNetCore** | Intégration ASP.NET Core |
| **Serilog.Sinks.Console** | Logs console |
| **Serilog.Sinks.File** | Logs fichiers |
| **Serilog.Sinks.ApplicationInsights** | Logs cloud (Azure) |

**Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

---

## 🐳 Containerisation & Orchestration

### Docker

| Technologie | Version | Usage |
|-------------|---------|-------|
| **Docker** | 24+ | Containerisation |
| **Docker Compose** | 2.20+ | Orchestration multi-conteneurs |
| **Kubernetes** | 1.28+ | Orchestration production (optionnel) |

**Avantages:**
- ✅ Éviter les conflits de ports
- ✅ Environnement reproductible
- ✅ Déploiement simplifié
- ✅ Isolation des services

**docker-compose.yml minimal:**
```yaml
version: '3.8'

services:
  gateway:
    build: ./DotnetNiger.Gateway
    ports:
      - "5000:80"
    depends_on:
      - identity-api
      - community-api
      - redis

  identity-api:
    build: ./DotnetNiger.Identity
    ports:
      - "5001:80"
    depends_on:
      - sqlserver
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=IdentityDb;User Id=sa;Password=YourPassword123!

  community-api:
    build: ./DotnetNiger.Community
    ports:
      - "5002:80"
    depends_on:
      - sqlserver
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CommunityDb;User Id=sa;Password=YourPassword123!

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    volumes:
      - sqlserver-data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  sqlserver-data:
```

---

## 🚪 Gateway & Reverse Proxy

### YARP (Yet Another Reverse Proxy)

| Composant | Version | Usage |
|-----------|---------|-------|
| **YARP** | 2.1+ | Reverse proxy Microsoft |
| **Polly** | 8.0+ | Circuit breaker & resilience |

**Alternative:** Ocelot (si YARP ne convient pas)

---

## 💾 Cache & Performance

### Redis

| Technologie | Version | Usage |
|-------------|---------|-------|
| **Redis** | 7+ | Cache distribué |
| **StackExchange.Redis** | 2.7+ | Client Redis pour .NET |

**Utilisé pour:**
- Cache des réponses GET
- Cache des catégories, tags, partners
- Rate limiting
- Session storage (optionnel)

---

## 📦 Packages NuGet Principaux

### Identity API

```xml
<ItemGroup>
  <!-- Core -->
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
  
  <!-- Authentication & Security -->
  <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
  
  <!-- Database -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  
  <!-- Validation & Mapping -->
  <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
  <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.0" />
  
  <!-- Caching -->
  <PackageReference Include="StackExchange.Redis" Version="2.7.0" />
  
  <!-- Logging -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  
  <!-- Documentation -->
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
</ItemGroup>
```

### Community API

```xml
<ItemGroup>
  <!-- Core -->
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
  
  <!-- Authentication -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  
  <!-- Database -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  
  <!-- Validation & Mapping -->
  <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
  <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.0" />
  
  <!-- Caching -->
  <PackageReference Include="StackExchange.Redis" Version="2.7.0" />
  
  <!-- Logging -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  
  <!-- Documentation -->
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
</ItemGroup>
```

### Gateway

```xml
<ItemGroup>
  <!-- Core -->
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
  
  <!-- YARP -->
  <PackageReference Include="Yarp.ReverseProxy" Version="2.1.0" />
  
  <!-- Authentication -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  
  <!-- Rate Limiting -->
  <PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
  
  <!-- Resilience -->
  <PackageReference Include="Polly" Version="8.2.0" />
  <PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
  
  <!-- Caching -->
  <PackageReference Include="StackExchange.Redis" Version="2.7.0" />
  
  <!-- Logging -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  
  <!-- Monitoring -->
  <PackageReference Include="prometheus-net.AspNetCore" Version="8.1.0" />
</ItemGroup>
```

---

## 🎨 Frontend (À définir)

### Option 1: React

| Technologie | Version | Usage |
|-------------|---------|-------|
| **React** | 18+ | Framework UI |
| **TypeScript** | 5+ | Typage statique |
| **Vite** | 5+ | Build tool |
| **React Router** | 6+ | Routing |
| **Axios** | 1.6+ | HTTP client |
| **TanStack Query** | 5+ | State management API |
| **Tailwind CSS** | 3+ | Styling |

---

### Option 2: Blazor

| Technologie | Version | Usage |
|-------------|---------|-------|
| **Blazor WebAssembly** | 8.0 | Framework UI .NET |
| **MudBlazor** | 6+ | Composants UI |
| **Blazored.LocalStorage** | 4+ | Storage local |

---

## 📊 Monitoring & Observabilité

### Production

| Technologie | Usage |
|-------------|-------|
| **Application Insights** | Monitoring Azure |
| **Prometheus** | Métriques |
| **Grafana** | Dashboard métriques |
| **Sentry** | Error tracking (optionnel) |
| **Health Checks** | Vérification santé services |

---

## 🧪 Tests

### Frameworks de Tests

| Technologie | Version | Usage |
|-------------|---------|-------|
| **xUnit** | 2.6+ | Framework de tests principal |
| **Moq** | 4.20+ | Mocking |
| **FluentAssertions** | 6.12+ | Assertions expressives |
| **WebApplicationFactory** | 8.0 | Tests d'intégration |
| **Bogus** | 35+ | Génération de données |

**Exemple de test:**
```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var request = new LoginRequest 
    { 
        Email = "user@test.com", 
        Password = "Password123!" 
    };

    // Act
    var result = await _authService.LoginAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.AccessToken.Should().NotBeNullOrEmpty();
}
```

---

## 🔧 Outils Requis

### Développement Local

| Outil | Version | Obligatoire |
|-------|---------|-------------|
| **.NET SDK** | 8.0 LTS | ✅ Oui |
| **Visual Studio** | 2022+ | ⚠️ Recommandé |
| **VS Code** | Latest | ✅ Alternative |
| **Git** | 2.40+ | ✅ Oui |
| **Docker Desktop** | 24+ | ✅ Oui |
| **SQL Server** | 2022+ | ⚠️ Ou via Docker |
| **Redis** | 7+ | ⚠️ Ou via Docker |
| **Postman** | Latest | ⚠️ Recommandé |

---

## 🚀 Extensions VS Code Recommandées

```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "ms-azuretools.vscode-docker",
    "ms-vscode-remote.remote-containers",
    "humao.rest-client",
    "streetsidesoftware.code-spell-checker",
    "editorconfig.editorconfig",
    "gruntfuggly.todo-tree"
  ]
}
```

---

## 📋 Best Practices

### Sécurité
- ✅ **Hasher tous les secrets** avant stockage BD
- ✅ **CORS restrictif** - Autoriser uniquement domaines connus
- ✅ **Rate limiting** sur auth endpoints
- ✅ **Logging** de tous les accès sensibles
- ✅ **Validation** côté backend systématiquement
- ✅ **Sanitization** de tout contenu UGC

### Performance
- ✅ **Pagination** obligatoire pour les listes
- ✅ **Projection** (Select) des DTOs
- ✅ **Caching** pour données statiques
- ✅ **Index** BD sur champs fréquents
- ✅ **AsNoTracking()** en lecture seule
- ✅ **Async/await** partout

### Code Quality
- ✅ **DTOs distincts** requêtes/réponses
- ✅ **AutoMapper** pour conversions
- ✅ **Guids** pour tous les IDs
- ✅ **Soft deletes** (champ DeletedAt)
- ✅ **Timestamps** (CreatedAt, UpdatedAt)
- ✅ **FluentValidation** pour validation

### Versioning
- ✅ **Semantic Versioning** (1.0.0)
- ✅ **EF Core Migrations** pour schéma
- ✅ **Backward compatibility**
- ✅ **Changelog** détaillé

---

## 🔗 Liens Utiles

- [Architecture](./03-ARCHITECTURE.md)
- [Setup](./01-SETUP.md)
- [Changelog](./16-CHANGELOG.md)

---

**Dernière mise à jour:** 29 Janvier 2026  
**Maintenu par:** Équipe DotnetNiger  
**.NET Version:** 8.0 LTS
