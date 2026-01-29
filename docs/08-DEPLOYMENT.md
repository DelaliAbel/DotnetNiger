# 🚀 Déploiement

Guide complet pour déployer DotnetNiger en production.

## 📋 Pré-requis

- Serveur avec Docker & Docker Compose
- Domaine avec DNS configuré
- Certificat SSL/TLS (Let's Encrypt)
- Base de données SQL Server
- Redis pour caching
- Compte pour CI/CD (GitHub Actions)

## 🐳 Déploiement avec Docker

### 1. Build des Images

```bash
# Build les images localement
docker-compose build

# Tagger les images
docker tag dotnetniger-gateway:latest myregistry.azurecr.io/dotnetniger-gateway:1.0.0
docker tag dotnetniger-identity:latest myregistry.azurecr.io/dotnetniger-identity:1.0.0
docker tag dotnetniger-community:latest myregistry.azurecr.io/dotnetniger-community:1.0.0
```

### 2. Push vers Registry

```bash
# Docker Hub
docker push myusername/dotnetniger-gateway:1.0.0
docker push myusername/dotnetniger-identity:1.0.0
docker push myusername/dotnetniger-community:1.0.0

# Azure Container Registry
docker push myregistry.azurecr.io/dotnetniger-gateway:1.0.0
```

### 3. Configuration Production

**docker-compose.prod.yml:**
```yaml
version: '3.8'

services:
  gateway:
    image: myregistry.azurecr.io/dotnetniger-gateway:1.0.0
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - Redis__Connection=${REDIS_CONNECTION}
      - Jwt__Secret=${JWT_SECRET}
    volumes:
      - /etc/letsencrypt/live/dotnetniger.com:/app/certs:ro
    depends_on:
      - identity-api
      - community-api
      - redis

  identity-api:
    image: myregistry.azurecr.io/dotnetniger-identity:1.0.0
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_IDENTITY}
      - Jwt__Secret=${JWT_SECRET}
    depends_on:
      - sqlserver

  community-api:
    image: myregistry.azurecr.io/dotnetniger-community:1.0.0
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_COMMUNITY}
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD}
    volumes:
      - sqlserver-prod-data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -Q "SELECT 1"
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis-prod-data:/data
    healthcheck:
      test: redis-cli ping
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  sqlserver-prod-data:
  redis-prod-data:
```

### 4. Lancer en Production

```bash
# Avec les variables d'environnement
docker-compose -f docker-compose.prod.yml up -d

# Vérifier le statut
docker-compose -f docker-compose.prod.yml ps

# Voir les logs
docker-compose -f docker-compose.prod.yml logs -f gateway
```

## ☁️ Azure App Service

### 1. Créer les Ressources

```bash
# Resource Group
az group create --name DotnetNiger --location eastus

# SQL Server
az sql server create \
  --resource-group DotnetNiger \
  --name dotnetniger-server \
  --admin-user sqluser \
  --admin-password ComplexPassword123!

# Databases
az sql db create \
  --resource-group DotnetNiger \
  --server dotnetniger-server \
  --name IdentityDb
```

### 2. App Services

```bash
# Gateway
az appservice plan create \
  --name DotnetNigerPlan \
  --resource-group DotnetNiger \
  --sku B2

az webapp create \
  --resource-group DotnetNiger \
  --plan DotnetNigerPlan \
  --name dotnetniger-gateway \
  --runtime "DOTNET:8.0"
```

### 3. Configurer l'App

```bash
# App Settings
az webapp config appsettings set \
  --resource-group DotnetNiger \
  --name dotnetniger-gateway \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Server=dotnetniger-server.database.windows.net;Database=IdentityDb;User Id=sqluser;Password=ComplexPassword123!"
```

## 🔐 SSL/TLS avec Let's Encrypt

```bash
# Installer Certbot
sudo apt-get install certbot python3-certbot-nginx

# Générer le certificat
sudo certbot certonly --standalone -d dotnetniger.com

# Certificat créé dans:
# /etc/letsencrypt/live/dotnetniger.com/fullchain.pem
# /etc/letsencrypt/live/dotnetniger.com/privkey.pem

# Auto-renouvellement
sudo systemctl enable certbot.timer
```

## 📊 Monitoring & Alertes

### Application Insights

```bash
# Créer Application Insights
az monitor app-insights component create \
  --app DotnetNiger-Insights \
  --resource-group DotnetNiger
```

### Ajouter à l'App

```csharp
builder.Services.AddApplicationInsightsTelemetry(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
```

## 🔄 CI/CD avec GitHub Actions

Voir `.github/workflows/deploy.yml` pour les détails complets.

### Déploiement Automatique

```yaml
name: Deploy

on:
  push:
    tags:
      - 'v*'

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker images
        run: docker-compose build
      
      - name: Push to registry
        run: |
          echo ${{ secrets.DOCKER_PASSWORD }} | docker login -u ${{ secrets.DOCKER_USERNAME }} --password-stdin
          docker push ...
      
      - name: Deploy to Azure
        run: |
          az login --service-principal -u ${{ secrets.AZURE_CLIENT_ID }} ...
          docker-compose -f docker-compose.prod.yml up -d
```

## 🧪 Tests Pré-Déploiement

### 1. Tests d'Intégration

```bash
dotnet test DotnetNiger.Tests --configuration Release
```

### 2. Performance

```bash
# Load testing avec Apache Bench
ab -n 1000 -c 100 https://dotnetniger.com/health
```

### 3. Sécurité

```bash
# OWASP ZAP scanning
zaproxy -cmd -quickurl https://dotnetniger.com
```

## 📋 Checklist Pré-Production

- ✅ Tous les tests passent
- ✅ Logs en place
- ✅ Monitoring configuré
- ✅ Backups automatiques
- ✅ Disaster recovery plan
- ✅ Secrets dans variables d'environnement
- ✅ Rate limiting activé
- ✅ CORS configuré
- ✅ HTTPS/TLS activé
- ✅ Alertes configurées

---

**Dernière mise à jour:** 29 Janvier 2026
