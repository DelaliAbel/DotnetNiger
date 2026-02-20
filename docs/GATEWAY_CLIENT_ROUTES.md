# Routes du Gateway - Consommation des Services

## Vue d'ensemble

Le Gateway expose des routes pour que les clients puissent consommer les services Identity et Community de manière cohérente et bien documentée.

## Architecture des Routes

```
Client Application
    ↓
Gateway (Port 5000)
    ├── /api/identity/* ────→ Identity Service (Port 5075)
    ├── /api/community/* ───→ Community Service (Port 5269)
    └── /api/gateway/* ─────→ Endpoints d'Agrégation
```

## Routes Identity Service (`/api/identity`)

### Authentication

#### Registration
```
POST /api/identity/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "fullName": "John Doe"
}

Response: 200 OK
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "...",
  "user": {
    "id": "user-123",
    "username": "johndoe",
    "email": "john@example.com",
    "fullName": "John Doe",
    "roles": []
  },
  "expiresIn": "2026-02-19T16:48:00Z"
}
```

#### Login
```
POST /api/identity/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123!"
}

Response: 200 OK
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "...",
  "user": {...},
  "expiresIn": "2026-02-19T16:48:00Z"
}

Response: 401 Unauthorized
{
  "message": "Identifiants invalides"
}
```

### User Management

#### Get Current User
```
GET /api/identity/users/me
Authorization: Bearer {accessToken}

Response: 200 OK
{
  "id": "user-123",
  "username": "johndoe",
  "email": "john@example.com",
  "fullName": "John Doe",
  "roles": ["user", "contributor"]
}

Response: 401 Unauthorized
{
  "message": "Token manquant"
}
```

#### Get User by ID
```
GET /api/identity/users/{userId}

Response: 200 OK
{
  "id": "user-123",
  "username": "johndoe",
  "email": "john@example.com",
  "fullName": "John Doe",
  "roles": ["user"]
}

Response: 404 Not Found
{
  "message": "Utilisateur non trouvé"
}
```

#### Get All Roles
```
GET /api/identity/roles

Response: 200 OK
[
  {
    "id": "role-1",
    "name": "Admin",
    "description": "Administrateur système"
  },
  {
    "id": "role-2",
    "name": "User",
    "description": "Utilisateur standard"
  },
  {
    "id": "role-3",
    "name": "Contributor",
    "description": "Contributeur de contenu"
  }
]
```

### Token Management

#### Validate Token
```
POST /api/identity/tokens/validate
Content-Type: application/json

{
  "token": "eyJhbGc..."
}

Response: 200 OK
{
  "valid": true,
  "message": "Token valide"
}
```

#### Refresh Token
```
POST /api/identity/tokens/refresh
Content-Type: application/json

{
  "refreshToken": "refresh-token-value"
}

Response: 200 OK
{
  "accessToken": "new-access-token",
  "refreshToken": "new-refresh-token",
  "user": {...},
  "expiresIn": "2026-02-19T17:48:00Z"
}

Response: 401 Unauthorized
{
  "message": "Refresh token invalide"
}
```

## Routes Community Service (`/api/community`)

### Posts Management

#### Get All Posts (Paginated)
```
GET /api/community/posts?page=1&pageSize=10

Query Parameters:
- page: int (default: 1) - Numéro de page
- pageSize: int (default: 10) - Nombre de posts par page (max: 100)

Response: 200 OK
{
  "source": "cache|service",
  "page": 1,
  "pageSize": 10,
  "data": [
    {
      "id": "post-123",
      "title": "Best Practices in .NET",
      "content": "Lorem ipsum...",
      "authorId": "user-456",
      "author": "John Doe",
      "createdAt": "2026-02-15T10:30:00Z",
      "likes": 45,
      "comments": 12
    }
  ]
}
```

**Caching**: La réponse est mise en cache pour 10 minutes.

#### Get Post by ID
```
GET /api/community/posts/{postId}

Response: 200 OK
{
  "source": "cache|service",
  "data": {
    "id": "post-123",
    "title": "Best Practices in .NET",
    "content": "Lorem ipsum...",
    "authorId": "user-456",
    "author": "John Doe",
    "createdAt": "2026-02-15T10:30:00Z",
    "likes": 45,
    "comments": 12
  }
}

Response: 404 Not Found
{
  "message": "Post non trouvé"
}
```

**Caching**: 30 minutes

#### Create Post
```
POST /api/community/posts
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "title": "New Article about Microservices",
  "content": "How to build scalable microservices...",
  "tags": ["microservices", "dotnet", "architecture"]
}

Response: 201 Created
Location: /api/community/posts/post-new-id
{
  "id": "post-new-id",
  "title": "New Article about Microservices",
  "content": "How to build scalable microservices...",
  "authorId": "user-123",
  "author": "John Doe",
  "createdAt": "2026-02-19T15:48:00Z",
  "likes": 0,
  "comments": 0
}

Response: 400 Bad Request
{
  "message": "Titre et contenu sont requis"
}
```

### Events Management

#### Get All Events
```
GET /api/community/events

Response: 200 OK
{
  "source": "cache|service",
  "data": [
    {
      "id": "event-123",
      "title": "DotNet Community Meetup",
      "description": "Join us for a community meetup...",
      "startDate": "2026-03-15T10:00:00Z",
      "endDate": "2026-03-15T12:00:00Z",
      "location": "Niamey",
      "attendees": 35
    }
  ]
}
```

**Caching**: 15 minutes

#### Get Event by ID
```
GET /api/community/events/{eventId}

Response: 200 OK
{
  "source": "cache|service",
  "data": {...}
}
```

**Caching**: 30 minutes

#### Create Event
```
POST /api/community/events
Content-Type: application/json
Authorization: Bearer {accessToken}

{
  "title": "Conference: Cloud Architecture",
  "description": "Learn about cloud architecture patterns...",
  "startDate": "2026-04-10T09:00:00Z",
  "endDate": "2026-04-10T17:00:00Z",
  "location": "Niamey Convention Center",
  "tags": ["conference", "cloud", "architecture"]
}

Response: 201 Created
{...}
```

### Projects Management

#### Get All Projects
```
GET /api/community/projects

Response: 200 OK
{
  "source": "cache|service",
  "data": [
    {
      "id": "project-123",
      "title": "DotNet Nigeria Website",
      "description": "Community website project...",
      "status": "In Progress",
      "teamMembers": ["user-1", "user-2", "user-3"]
    }
  ]
}
```

**Caching**: 20 minutes

#### Get Project by ID
```
GET /api/community/projects/{projectId}

Response: 200 OK
{...}
```

**Caching**: 30 minutes

### Categories & Resources

#### Get All Categories
```
GET /api/community/categories

Response: 200 OK
{
  "source": "cache|service",
  "data": [
    {
      "id": "cat-1",
      "name": "Tutorials",
      "description": "Step-by-step tutorials",
      "itemCount": 42
    },
    {
      "id": "cat-2",
      "name": "Best Practices",
      "description": "Industry best practices",
      "itemCount": 28
    }
  ]
}
```

**Caching**: 1 hour

#### Get All Resources
```
GET /api/community/resources

Response: 200 OK
{
  "source": "cache|service",
  "data": [
    {
      "id": "res-1",
      "title": "Microsoft Documentation",
      "url": "https://docs.microsoft.com/",
      "type": "Documentation"
    }
  ]
}
```

**Caching**: 2 hours

### Search

#### Search Content
```
GET /api/community/search?query=microservices

Query Parameters:
- query: string (required) - Search term

Response: 200 OK
{
  "source": "cache|service",
  "data": {
    "posts": [
      {
        "id": "post-1",
        "title": "Building Microservices with .NET",
        ...
      }
    ],
    "events": [
      {
        "id": "event-1",
        "title": "Microservices Workshop",
        ...
      }
    ],
    "projects": [
      {
        "id": "project-1",
        "title": "Microservices Framework",
        ...
      }
    ],
    "totalResults": 35
  }
}

Response: 400 Bad Request
{
  "message": "Terme de recherche requis"
}
```

**Caching**: 5 minutes

## Routes d'Agrégation du Gateway (`/api/gateway`)

### Health Check

#### Gateway Status
```
GET /api/gateway/status

Response: 200 OK
{
  "status": "Gateway is running",
  "timestamp": "2026-02-19T15:48:00Z",
  "services": {
    "identity": "Available at /identity",
    "community": "Available at /community",
    "swagger": "Available at /swagger"
  }
}
```

#### Gateway Statistics
```
GET /api/gateway/stats

Response: 200 OK
{
  "message": "Statistics endpoint",
  "gateway": "Monitoring gateway performance and microservice health",
  "endpoints": {
    "identity": "/identity",
    "community": "/community",
    "swagger": "/swagger"
  }
}
```

## Reverse Proxy Routes (Low-level)

These routes are automatically routed by YARP to the backend services:

```
GET|POST|PUT|DELETE /identity/*  → [Identity Service]
GET|POST|PUT|DELETE /community/* → [Community Service]
GET                  /swagger/identity/*  → [Identity Service Swagger] (with path transform)
GET                  /swagger/community/* → [Community Service Swagger] (with path transform)
```

## Error Handling

Tous les endpoints retournent les codes HTTP standard:

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Requête réussie |
| 201 | Created | Ressource créée |
| 400 | Bad Request | Paramètres invalides |
| 401 | Unauthorized | Token manquant ou invalide |
| 404 | Not Found | Ressource non trouvée |
| 500 | Internal Server Error | Erreur du serveur |

Tous les erreurs retournent:
```json
{
  "message": "Description de l'erreur"
}
```

## Caching Strategy

Le Gateway applique une stratégie de caching intelligente:

| Endpoint | Cache Duration | Notes |
|----------|-----------------|-------|
| `/api/community/posts` | 10 minutes | Liste paginée |
| `/api/community/posts/{id}` | 30 minutes | Post unique |
| `/api/community/events` | 15 minutes | Tous les événements |
| `/api/community/events/{id}` | 30 minutes | Événement unique |
| `/api/community/projects` | 20 minutes | Tous les projets |
| `/api/community/projects/{id}` | 30 minutes | Projet unique |
| `/api/community/categories` | 1 hour | Rarement changé |
| `/api/community/resources` | 2 hours | Rarement changé |
| `/api/community/search` | 5 minutes | Résultats dynamiques |

**Note**: Le cache est invalidé automatiquement lors de la création de nouvelles ressources.

## Rate Limiting

Le Gateway applique un rate limiting:
- **Limite**: 100 requêtes par minute par défaut
- **Application**: Par client (basé sur IP ou utilisateur)
- **Fallback**: Retourne 429 Too Many Requests si dépassé

Configuré dans `appsettings.json`:
```json
{
  "RateLimit": {
    "MaxRequestsPerMinute": 100
  }
}
```

## Authentication

Pour les endpoints protégés (marqués par 🔒):

```
Authorization: Bearer {jwt-token}
```

Obtênez un token via:
```
POST /api/identity/auth/login
```

## Examples

### Complete Authentication Flow

```bash
# 1. Register
curl -X POST http://localhost:5000/api/identity/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "SecurePassword123!",
    "fullName": "New User"
  }'

# 2. Login
TOKEN=$(curl -X POST http://localhost:5000/api/identity/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePassword123!"
  }' | jq -r '.accessToken')

# 3. Use token
curl -X GET http://localhost:5000/api/identity/users/me \
  -H "Authorization: Bearer $TOKEN"
```

### Create Post with Authentication

```bash
curl -X POST http://localhost:5000/api/community/posts \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "My First Post",
    "content": "This is my first post on the community platform",
    "tags": ["dotnet", "community"]
  }'
```

## Documentation

Pour plus d'informations:
- Swagger UI: `http://localhost:5000/swagger`
- Gateway Documentation: [README.md](./README.md)
- Service Identity: `http://localhost:5075/swagger`
- Service Community: `http://localhost:5269/swagger`

---

**Version**: 1.0  
**Last Updated**: February 19, 2026
