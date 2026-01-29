# 📦 SETUP Guide

Guide d'installation et configuration de DotnetNiger.

Pour la documentation **complète et détaillée**, consulter [`/docs/01-SETUP.md`](./docs/01-SETUP.md).

---

## 🚀 Quick Start (30 secondes)

```bash
# 1. Clone le repository
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger

# 2. Restaure les dépendances
dotnet restore

# 3. Lance les services
docker-compose up -d

# 4. Accède à l'application
# Gateway: https://localhost:5000
# Identity: https://localhost:5001
# Community: https://localhost:5002
```

---

## 📋 Prérequis

- .NET 8.0 LTS SDK
- Docker & Docker Compose
- SQL Server 2022 (ou via Docker)
- Redis (ou via Docker)
- Git

---

## 🔧 Configuration

### 1. Environment Variables

```bash
# Crée .env file ou configure variables d'environnement
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection=Server=localhost;Database=DotnetNiger;...
export JWT_SECRET=your-secret-key
```

### 2. Database

```bash
# Applique les migrations
dotnet ef database update --project DotnetNiger.Identity
dotnet ef database update --project DotnetNiger.Community
```

### 3. Docker (Recommandé)

```bash
# Lance tous les services
docker-compose up -d

# Vérifie le statut
docker-compose ps

# Vois les logs
docker-compose logs -f
```

---

## ✅ Vérification

```bash
# Health check
curl https://localhost:5000/gateway/health

# API disponible?
curl https://localhost:5000/swagger/index.html
```

---

## 📚 Documentation Complète

Pour plus de détails, consulter:
- **Installation détaillée:** [`/docs/01-SETUP.md`](./docs/01-SETUP.md)
- **Quick Start 5 min:** [`/docs/02-QUICK-START.md`](./docs/02-QUICK-START.md)
- **Architecture:** [`/docs/03-ARCHITECTURE.md`](./docs/03-ARCHITECTURE.md)
- **Stack technique:** [`/docs/04-TECHNICAL-STACK.md`](./docs/04-TECHNICAL-STACK.md)

---

**Pour la documentation complète, voir [`/docs/`](./docs/)**
