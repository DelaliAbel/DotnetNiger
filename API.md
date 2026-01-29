# API Documentation

Documentation détaillée de toutes les APIs disponibles.

## 📋 Vue d'ensemble

DotnetNiger expose 3 services API :

| Service | Port | Description |
|---------|------|-------------|
| Gateway | 5000 | Agrégateur et point d'entrée |
| Identity | 5075 | Authentification et gestion des utilisateurs |
| Community | 5269 | Contenus et interactions sociales |

## 🔐 Authentication

### JWT Token

Tous les endpoints (sauf les publics) nécessitent un JWT token.

**Format:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token Payload:**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "roles": ["user"],
  "iat": 1234567890,
  "exp": 1234571490
}
```

**Durée de validité:** 60 minutes

## 🔌 Identity Service

Base URL: `http://localhost:5075`

### Authentication Endpoints

#### 1. Register User
```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "User",
  "lastName": "Sample"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "email": "user@example.com",
  "firstName": "User",
  "lastName": "Sample",
  "createdAt": "2026-01-29T10:00:00Z"
}
```

#### 2. Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `400 Bad Request` - Email ou mot de passe invalide
- `401 Unauthorized` - Credentials incorrects

#### 3. Refresh Token
```http
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600
}
```

#### 4. Logout
```http
POST /auth/logout
Authorization: Bearer <token>
```

**Response (204 No Content)**

### User Endpoints

#### 1. Get Profile
```http
GET /auth/profile
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "createdAt": "2026-01-29T10:00:00Z",
  "lastLoginAt": "2026-01-29T10:30:00Z"
}
```

#### 2. Update Profile
```http
PUT /auth/profile
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jean",
  "lastName": "Martin"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "email": "user@example.com",
  "firstName": "Jean",
  "lastName": "Martin",
  "updatedAt": "2026-01-29T10:31:00Z"
}
```

#### 3. Change Password
```http
POST /auth/change-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password updated successfully"
}
```

## 👥 Community Service

Base URL: `http://localhost:5269`

### Posts Endpoints

#### 1. List Posts
```http
GET /posts?page=1&pageSize=10&sortBy=createdAt&sortDirection=desc
Authorization: Bearer <token>
```

**Query Parameters:**
- `page` (default: 1) - Numéro de page
- `pageSize` (default: 10) - Nombre de posts par page
- `sortBy` (default: createdAt) - Champ de tri
- `sortDirection` (default: desc) - asc ou desc

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "title": "Mon premier post",
      "content": "Contenu du post...",
      "authorId": 1,
      "authorName": "John Doe",
      "likeCount": 5,
      "commentCount": 2,
      "createdAt": "2026-01-29T10:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "total": 42,
    "totalPages": 5
  }
}
```

#### 2. Get Post by ID
```http
GET /posts/{id}
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "title": "Mon premier post",
  "content": "Contenu du post...",
  "authorId": 1,
  "authorName": "John Doe",
  "likeCount": 5,
  "commentCount": 2,
  "createdAt": "2026-01-29T10:00:00Z",
  "comments": [
    {
      "id": 1,
      "authorId": 2,
      "authorName": "Jane Doe",
      "content": "Super post!",
      "createdAt": "2026-01-29T10:05:00Z"
    }
  ]
}
```

#### 3. Create Post
```http
POST /posts
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Nouveau post",
  "content": "Contenu du post..."
}
```

**Response (201 Created):**
```json
{
  "id": 43,
  "title": "Nouveau post",
  "content": "Contenu du post...",
  "authorId": 1,
  "createdAt": "2026-01-29T10:35:00Z"
}
```

#### 4. Update Post
```http
PUT /posts/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Post modifié",
  "content": "Contenu modifié..."
}
```

**Response (200 OK):**
```json
{
  "id": 43,
  "title": "Post modifié",
  "content": "Contenu modifié...",
  "updatedAt": "2026-01-29T10:36:00Z"
}
```

#### 5. Delete Post
```http
DELETE /posts/{id}
Authorization: Bearer <token>
```

**Response (204 No Content)**

### Comments Endpoints

#### 1. Get Comments (pour un post)
```http
GET /posts/{postId}/comments
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "postId": 1,
      "authorId": 2,
      "authorName": "Jane Doe",
      "content": "Super post!",
      "createdAt": "2026-01-29T10:05:00Z"
    }
  ]
}
```

#### 2. Create Comment
```http
POST /posts/{postId}/comments
Authorization: Bearer <token>
Content-Type: application/json

{
  "content": "Commentaire super cool!"
}
```

**Response (201 Created):**
```json
{
  "id": 2,
  "postId": 1,
  "authorId": 1,
  "content": "Commentaire super cool!",
  "createdAt": "2026-01-29T10:37:00Z"
}
```

#### 3. Update Comment
```http
PUT /posts/{postId}/comments/{commentId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "content": "Commentaire modifié!"
}
```

**Response (200 OK):**
```json
{
  "id": 2,
  "postId": 1,
  "content": "Commentaire modifié!",
  "updatedAt": "2026-01-29T10:38:00Z"
}
```

#### 4. Delete Comment
```http
DELETE /posts/{postId}/comments/{commentId}
Authorization: Bearer <token>
```

**Response (204 No Content)**

### Likes Endpoints

#### 1. Like Post
```http
POST /posts/{id}/like
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "postId": 1,
  "userId": 1,
  "liked": true,
  "likeCount": 6
}
```

#### 2. Unlike Post
```http
POST /posts/{id}/unlike
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "postId": 1,
  "userId": 1,
  "liked": false,
  "likeCount": 5
}
```

## 🌐 Gateway Aggregated APIs

Base URL: `http://localhost:5000`

### Gateway Health Endpoints

#### 1. Health Check
```http
GET /health
```

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "services": {
    "gateway": "Healthy",
    "identity": "Healthy",
    "community": "Healthy",
    "database": "Healthy",
    "redis": "Healthy"
  },
  "timestamp": "2026-01-29T10:40:00Z"
}
```

#### 2. Metrics
```http
GET /metrics
```

**Response (200 OK - Prometheus format):**
```
# HELP requests_total Total HTTP requests
# TYPE requests_total counter
requests_total{method="GET",endpoint="/posts"} 105
requests_total{method="POST",endpoint="/posts"} 24

# HELP response_time_ms Response time in milliseconds
# TYPE response_time_ms histogram
response_time_ms_bucket{endpoint="/posts",le="100"} 95
```

### Gateway Aggregated Swagger
```http
GET /swagger/v1/swagger.json
```

Retourne la spécification OpenAPI agrégée de tous les services.

## ❌ Error Responses

Tous les services utilisent le même format d'erreur :

```json
{
  "statusCode": 400,
  "message": "Invalid request",
  "details": "Email is required",
  "timestamp": "2026-01-29T10:41:00Z"
}
```

### Status Codes communs

| Code | Description |
|------|-------------|
| 200 | OK |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 429 | Too Many Requests (Rate Limited) |
| 500 | Internal Server Error |
| 503 | Service Unavailable |

## 🔄 Rate Limiting

**Limites par défaut:**
- 100 requêtes par minute par utilisateur
- 1000 requêtes par minute par IP

**Headers de réponse:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1234567890
```

**Quand limité (429):**
```json
{
  "statusCode": 429,
  "message": "Rate limit exceeded",
  "retryAfter": 60
}
```

## 🧪 Test avec cURL

```bash
# Register
curl -X POST http://localhost:5075/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'

# Login et récupérer token
TOKEN=$(curl -s -X POST http://localhost:5075/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}' \
  | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)

# Utiliser le token
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:5269/posts
```

## 📚 Ressources

- [Swagger UI - Gateway](http://localhost:5000/swagger)
- [Swagger UI - Identity](http://localhost:5075/swagger)
- [Swagger UI - Community](http://localhost:5269/swagger)
- [OpenAPI 3.0 Specification](https://spec.openapis.org/oas/v3.0.3)

---

Pour l'intégration, consulter les collections Postman fournies.
