# API

Base URL (dev): <http://localhost:5000>

## Auth

Tous les endpoints (sauf auth) exigent:

```
Authorization: Bearer <token>
```

Login example:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

## Identity

- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/refresh
- POST /api/auth/logout
- POST /api/auth/forgot-password
- POST /api/auth/reset-password
- POST /api/auth/verify-email
- GET /api/users/me
- PUT /api/users/me
- POST /api/users/me/change-password
- POST /api/users/me/change-email
- GET /api/social-links
- POST /api/social-links
- DELETE /api/social-links/{id}
- GET /api/roles
- POST /api/roles
- DELETE /api/roles/{id}
- POST /api/roles/assign
- POST /api/roles/remove
- GET /api/roles/user/{userId}
- GET /api/permissions
- POST /api/permissions
- DELETE /api/permissions/{id}
- POST /api/permissions/assign
- POST /api/permissions/remove
- GET /api/permissions/role/{roleId}
- GET /api/admin/users
- GET /api/admin/users/{userId}
- PUT /api/admin/users/{userId}/status

## Community

- POST /api/posts
- GET /api/posts
- GET /api/posts/{postId}
- PUT /api/posts/{postId}
- DELETE /api/posts/{postId}
- POST /api/posts/{postId}/comments
- GET /api/posts/{postId}/comments
- POST /api/posts/{postId}/like
- DELETE /api/posts/{postId}/like
- POST /api/users/{userId}/follow
- GET /api/users/{userId}/followers
- GET /api/feed

## Gateway

- GET /health
- GET /swagger/v1/swagger.json
- GET /swagger-aggregated/v1/swagger.json (si active)
- GET /metrics (si active)

## Errors (standard)

- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 404 Not Found
- 429 Too Many Requests
- 500 Internal Server Error

````

#### Get Followers

```http
GET /api/users/{userId}/followers?skip=0&take=20
Authorization: Bearer <token>
````

**Response:** `200 OK`

### Feed

#### Get Personal Feed

```http
GET /api/feed?skip=0&take=20
Authorization: Bearer <token>
```

**Response:** `200 OK` - Posts des utilisateurs suivis

---

## 🏥 Gateway

### Health

#### Gateway Health

```http
GET /health
```

**Response:** `200 OK`

```json
{
  "status": "Healthy",
  "services": {
    "identity": "Healthy",
    "community": "Healthy"
  }
}
```

### Swagger

#### Aggregated Swagger

```http
GET /swagger/ui
GET /swagger/v1/swagger.json
```

### Metrics

#### Prometheus Metrics

```http
GET /metrics
```

**Response:** Format Prometheus

---

## ⚠️ Error Responses

### 400 Bad Request

```json
{
  "error": "Bad Request",
  "message": "Invalid input",
  "details": {
    "email": "Email is invalid"
  }
}
```

### 401 Unauthorized

```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing token"
}
```

### 403 Forbidden

```json
{
  "error": "Forbidden",
  "message": "You don't have permission"
}
```

### 404 Not Found

```json
{
  "error": "Not Found",
  "message": "Resource not found"
}
```

### 429 Too Many Requests

```json
{
  "error": "Too Many Requests",
  "message": "Rate limit exceeded",
  "retryAfter": 60
}
```

### 500 Internal Server Error

```json
{
  "error": "Internal Server Error",
  "message": "An unexpected error occurred",
  "traceId": "0HN0E6HPVLF5Q:00000001"
}
```

---

## 🧪 Testing avec cURL

### Register

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!"
  }'
```

### Create Post

```bash
curl -X POST http://localhost:5000/api/posts \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Post",
    "content": "Test content",
    "tags": ["test"]
  }'
```

---

**Dernière mise à jour:** 29 Janvier 2026
