# 🚀 Démarrage Rapide

Lancez DotnetNiger en 5 minutes!

## Option 1: Docker Compose (Recommandé)

### Prérequis

- Docker & Docker Compose installés
- 4GB RAM disponible

### Étapes

```bash
# 1. Cloner le repository
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger

# 2. Lancer les services
docker-compose up -d

# 3. Attendre le démarrage (30-60 secondes)
docker-compose logs -f

# 4. Vérifier la santé
docker-compose ps
```

### Accès

```
Gateway:    http://localhost:5000/swagger
Identity:   http://localhost:5001/swagger
Community:  http://localhost:5002/swagger
```

### Test

```bash
# Créer un utilisateur
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!"
  }'

# Obtenir un token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!"
  }'
```

---

## Option 2: Local Development

### Prérequis

- .NET 8.0 SDK ou supérieur
- SQL Server 2022 (ou Express)
- Redis 7+
- Git

### Installation

```bash
# 1. Cloner
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger

# 2. Restaurer les dépendances
dotnet restore

# 3. Configurer BD
# Modifiez appsettings.Development.json avec vos connexions

# 4. Migrations
cd DotnetNiger.Identity
dotnet ef database update
cd ../DotnetNiger.Community
dotnet ef database update
cd ..

# 5. Lancer les services (dans 3 terminals différents)
# Terminal 1
dotnet run --project DotnetNiger.Gateway

# Terminal 2
dotnet run --project DotnetNiger.Identity

# Terminal 3
dotnet run --project DotnetNiger.Community
```

### Accès

```
Gateway:    http://localhost:5000/swagger
Identity:   http://localhost:5001/swagger
Community:  http://localhost:5002/swagger
```

---

## Arrêter les Services

### Docker

```bash
docker-compose down
```

### Local

```
Ctrl+C dans chaque terminal
```

---

## Troubleshooting

### Ports occupés

```bash
# Trouver le processus qui utilise le port 5000
netstat -ano | findstr :5000
```

### BD non accessible

```bash
# Vérifier la connexion SQL Server
sqlcmd -S localhost -U sa -P YourPassword
```

### Redis non disponible

```bash
# Vérifier Redis
redis-cli ping
```

---

✅ **Vous êtes prêt!** Consultez [SETUP.md](./01-SETUP.md) pour plus de détails.
