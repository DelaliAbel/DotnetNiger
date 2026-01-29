# ⚙️ Configuration & Installation

Guide complet de configuration pour DotnetNiger.

## 📋 Prérequis

### Système
- Windows 10/11, macOS 11+, ou Linux (Ubuntu 20.04+)
- 8GB RAM minimum, 16GB recommandé
- 20GB espace disque

### Logiciels
- **.NET SDK 8.0 LTS** - [Télécharger](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **Git 2.40+** - [Télécharger](https://git-scm.com/download)
- **Docker & Docker Compose** - [Télécharger](https://www.docker.com/products/docker-desktop)

### Bases de Données & Caching
- **SQL Server 2022** (ou Microsoft SQL Server Express)
- **Redis 7+**

---

## 🛠️ Installation

### 1. Cloner le Repository

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
```

### 2. Configuration Locale

#### Fichiers de Configuration

Chaque service a un fichier `appsettings.Development.json`:

```bash
DotnetNiger.Identity/appsettings.Development.json
DotnetNiger.Community/appsettings.Development.json
DotnetNiger.Gateway/appsettings.Development.json
```

#### Modèle de Configuration (Identity)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IdentityDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-min-32-chars-required",
    "Issuer": "dotnetniger",
    "Audience": "dotnetniger-api",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Redis": {
    "Connection": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### 3. Restaurer les Dépendances

```bash
dotnet restore
```

### 4. Migrations Base de Données

```bash
# Identity
cd DotnetNiger.Identity
dotnet ef database update
cd ..

# Community
cd DotnetNiger.Community
dotnet ef database update
cd ..
```

### 5. Lancer les Services

#### Avec Docker Compose (Recommandé)

```bash
docker-compose up -d
```

Vérifier le statut:
```bash
docker-compose ps
```

#### Localement

Terminal 1 - Gateway:
```bash
cd DotnetNiger.Gateway
dotnet run
```

Terminal 2 - Identity:
```bash
cd DotnetNiger.Identity
dotnet run
```

Terminal 3 - Community:
```bash
cd DotnetNiger.Community
dotnet run
```

---

## 🔧 Configuration Avancée

### Variables d'Environnement

```bash
# Identity Service
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5001
export ConnectionStrings__DefaultConnection=...

# Community Service
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5002
export ConnectionStrings__DefaultConnection=...

# Gateway
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5000
```

### CORS

Modifiez `Program.cs` pour autoriser les domaines:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder
                .WithOrigins("http://localhost:3000", "https://yourdomain.com")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

app.UseCors("AllowSpecificOrigins");
```

### Rate Limiting

Configurez dans `appsettings.json`:

```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "HttpStatusCode": 429,
    "IpWhitelist": ["127.0.0.1"],
    "ClientWhitelist": [],
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      },
      {
        "Endpoint": "*:/api/auth/*",
        "Period": "1m",
        "Limit": 10
      }
    ]
  }
}
```

### Logging

Configurez Serilog dans `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", environment)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

---

## 🐳 Configuration Docker

### Personnaliser docker-compose.yml

```yaml
version: '3.8'

services:
  gateway:
    build:
      context: .
      dockerfile: DotnetNiger.Gateway/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - identity-api
      - community-api
      - redis
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourPassword123!"
    ports:
      - "1433:1433"
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

## ✅ Vérifier l'Installation

### 1. Services Opérationnels

```bash
docker-compose ps
```

Tous les conteneurs doivent être "Up".

### 2. Swagger Accessible

```
http://localhost:5000/swagger  # Gateway
http://localhost:5001/swagger  # Identity
http://localhost:5002/swagger  # Community
```

### 3. Health Check

```bash
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
```

Réponse attendue:
```json
{
  "status": "Healthy",
  "checks": {}
}
```

---

## 🆘 Troubleshooting

### Erreur de Port

**Problème:** "Address already in use"

**Solution:**
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :5000
kill -9 <PID>
```

### Erreur de Connexion BD

**Problème:** "Cannot connect to database"

**Solution:**
1. Vérifier SQL Server est lancé
2. Vérifier la chaîne de connexion
3. Vérifier les identifiants

```bash
sqlcmd -S localhost -U sa -P YourPassword123!
```

### Redis Non Disponible

**Problème:** "Connection refused to Redis"

**Solution:**
```bash
redis-cli ping  # Doit retourner PONG
```

### Migrations Échouées

**Problème:** "Unable to apply migrations"

**Solution:**
```bash
# Réinitialiser la BD
dotnet ef database drop --force
dotnet ef database update
```

---

## 📚 Fichiers de Configuration

- [01-SETUP.md](./01-SETUP.md) - Ce fichier
- [02-QUICK-START.md](./02-QUICK-START.md) - Démarrage rapide
- [04-TECHNICAL-STACK.md](./04-TECHNICAL-STACK.md) - Stack technique

---

**Dernière mise à jour:** 29 Janvier 2026
