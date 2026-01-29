# 🏗️ Architecture du Projet

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
   - Gère les rôles et permissions

3. **Rate Limiting**
   - Protège contre les abus
   - Limites par endpoint et utilisateur
   - Configuration flexible

4. **Caching**
   - Cache distribué (Redis)
   - Améliore les performances
   - Cohérence des données

5. **Aggregation Swagger**
   - Agrège les spécifications OpenAPI
   - Documentation centralisée
   - Interface Swagger unique

## 🔐 DotnetNiger.Identity

### Responsabilités

- **Gestion des Utilisateurs**
  - Création, modification, suppression
  - Profils utilisateurs
  - Roles et permissions

- **Authentification**
  - Login/Logout
  - Refresh tokens
  - Password reset

- **Validation**
  - Email verification
  - Password policies
  - 2FA (optionnel)

- **Sécurité**
  - Hashing des mots de passe (PBKDF2)
  - JWT signing
  - Rate limiting sur auth

## 👥 DotnetNiger.Community

### Responsabilités

- **Gestion des Publications**
  - Créer, modifier, supprimer posts
  - Pagination, filtrage, recherche

- **Commentaires**
  - Ajouter commentaires sur posts
  - Threading de commentaires
  - Modération

- **Interactions**
  - Likes/Dislikes
  - Followers/Following
  - Notifications

- **Feed**
  - Algorithme feed personnalisé
  - Posts des utilisateurs suivis
  - Trending topics

## 💾 Couche Données

### Entity Framework Core

- ORM pour accès données
- Migrations versionnées
- Lazy loading vs Eager loading

### Repositories

```csharp
public interface IRepository<T>
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync(int skip = 0, int take = 10);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync();
}
```

## 🔄 Flux de Requête

```
1. Client envoie requête
   ↓
2. Gateway (YARP) reçoit
   ↓
3. Authentication Middleware valide JWT
   ↓
4. Rate Limiting vérifie limites
   ↓
5. Cache (Redis) cherche réponse
   ↓
6. Si miss: Route vers Service approprié
   ↓
7. Service (Business Logic)
   ↓
8. Repository (Data Access)
   ↓
9. Database (SQL Server)
   ↓
10. Réponse remonte par la même chaîne
   ↓
11. Cache stocke réponse
   ↓
12. Client reçoit réponse
```

## 🔒 Sécurité

### Authentification

```csharp
// JWT Bearer Token
Authorization: Bearer eyJhbGc...
```

### Validation des Inputs

- FluentValidation côté serveur
- Sanitization de contenu UGC
- Protection SQL Injection (EF Core paramétré)

### CORS

- Strict par défaut
- Domaines autorisés uniquement
- Credentials conditionnels

## 📊 Données Partagées

Chaque service a sa **propre base de données**:

- Pas de partage direct
- Communication via API Gateway
- Event-sourcing optionnel

## 🚀 Performance

### Caching Strategy

```
L1: Application Memory (MemoryCache)
L2: Distributed Cache (Redis)
L3: Database
```

### Pagination

Toutes les listes sont paginées:

```csharp
// Request
GET /api/posts?skip=0&take=20

// Response
{
  "data": [...],
  "total": 1500,
  "skip": 0,
  "take": 20
}
```

---

**Dernière mise à jour:** 29 Janvier 2026
