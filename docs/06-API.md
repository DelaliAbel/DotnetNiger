# 📡 Documentation API Complète

Documentation détaillée de tous les endpoints.

## 🌐 Base URLs

- **Development:** `http://localhost:5000`

## 🔐 Authentication

Tous les endpoints (sauf auth) nécessitent un JWT Bearer token:

```bash
Authorization: Bearer <token>
```

### Obtenir un Token

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 3600
}
```

---

## 👤 Identity Service

### Authentification

#### 1. Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `201 Created`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "newuser@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### 2. Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response:** `200 OK`

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 3600
}
```

#### 3. Refresh Token

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGc..."
}
```

**Response:** `200 OK`

```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "eyJhbGc...",
  "expiresIn": 3600
}
```

#### 4. Logout

```http
POST /api/auth/logout
Authorization: Bearer <token>
```

**Response:** `204 No Content`

### Profils Utilisateurs

#### Get Profile

```http
GET /api/users/profile
Authorization: Bearer <token>
```

**Response:** `200 OK`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "bio": "A passionate developer",
  "avatarUrl": "https://...",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T15:45:00Z"
}
```

#### Update Profile

```http
PUT /api/users/profile
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "bio": "Updated bio",
  "avatarUrl": "https://..."
}
```

**Response:** `200 OK`

---

## 📝 Community Service

### Posts

#### Create Post

```http
POST /api/posts
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "My First Post",
  "content": "This is the content of my post",
  "tags": ["dotnet", "api"]
}
```

**Response:** `201 Created`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "My First Post",
  "content": "This is the content of my post",
  "authorId": "550e8400-e29b-41d4-a716-446655440001",
  "tags": ["dotnet", "api"],
  "likes": 0,
  "comments": 0,
  "createdAt": "2024-01-20T10:30:00Z"
}
```

#### Get Posts

```http
GET /api/posts?skip=0&take=20&sortBy=createdAt&sortOrder=desc
Authorization: Bearer <token>
```

**Response:** `200 OK`

```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "title": "My First Post",
      "content": "This is the content of my post",
      "authorId": "550e8400-e29b-41d4-a716-446655440001",
      "authorName": "John Doe",
      "tags": ["dotnet", "api"],
      "likes": 5,
      "comments": 2,
      "createdAt": "2024-01-20T10:30:00Z"
    }
  ],
  "total": 150,
  "skip": 0,
  "take": 20
}
```

#### Get Post Detail

```http
GET /api/posts/{postId}
Authorization: Bearer <token>
```

**Response:** `200 OK`

#### Update Post

```http
PUT /api/posts/{postId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Updated Title",
  "content": "Updated content"
}
```

**Response:** `200 OK`

#### Delete Post

```http
DELETE /api/posts/{postId}
Authorization: Bearer <token>
```

**Response:** `204 No Content`

### Comments

#### Add Comment

```http
POST /api/posts/{postId}/comments
Authorization: Bearer <token>
Content-Type: application/json

{
  "content": "Great post!",
  "parentCommentId": null
}
```

**Response:** `201 Created`

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "postId": "550e8400-e29b-41d4-a716-446655440000",
  "authorId": "550e8400-e29b-41d4-a716-446655440001",
  "content": "Great post!",
  "createdAt": "2024-01-20T11:00:00Z"
}
```

#### Get Comments

```http
GET /api/posts/{postId}/comments?skip=0&take=10
Authorization: Bearer <token>
```

**Response:** `200 OK`

### Interactions

#### Like Post

```http
POST /api/posts/{postId}/like
Authorization: Bearer <token>
```

**Response:** `200 OK`

```json
{
  "liked": true,
  "likeCount": 6
}
```

#### Unlike Post

```http
DELETE /api/posts/{postId}/like
Authorization: Bearer <token>
```

**Response:** `200 OK`

### Follow/Unfollow

#### Follow User

```http
POST /api/users/{userId}/follow
Authorization: Bearer <token>
```

**Response:** `200 OK`

```json
{
  "following": true,
  "followerCount": 25
}
```

#### Get Followers

```http
GET /api/users/{userId}/followers?skip=0&take=20
Authorization: Bearer <token>
```

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
