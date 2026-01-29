# 🚀 DEPLOYMENT Guide

Guide de déploiement de DotnetNiger en production.

Pour la documentation **complète et détaillée**, consulter [`/docs/08-DEPLOYMENT.md`](./docs/08-DEPLOYMENT.md).

---

## 🎯 Options de Déploiement

### Option 1: Docker Compose (Local/Staging)

```bash
# Build images
docker-compose build

# Déploie en production
docker-compose -f docker-compose.yml up -d

# Vérifie le statut
docker-compose ps
```

### Option 2: Kubernetes

```bash
# Crée le cluster
kubectl apply -f k8s/

# Vérifie le statut
kubectl get pods
kubectl get services
```

### Option 3: Azure Container Instances

```bash
# Déploie sur Azure
az container create \
  --resource-group mygroup \
  --name dotnetniger-api \
  --image dotnetniger:latest
```

---

## 🔐 Configuration Production

### Secrets Management

```bash
# Azure Key Vault
az keyvault secret set --vault-name mykeyvault \
  --name "ConnectionString" \
  --value "Server=...;Password=..."

# Environment variables
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET=$(az keyvault secret show --vault-name mykeyvault --name JwtSecret --query value -o tsv)
```

### Database

```bash
# Migrate production database
dotnet ef database update \
  --configuration Release \
  --connection "Server=prod-server;Database=DotnetNiger;..."
```

### SSL/TLS

```bash
# Générer certificat (Let's Encrypt)
certbot certonly --standalone -d api.dotnetniger.com

# Configure dans Nginx/IIS
```

---

## 📊 Monitoring

```bash
# Health check
curl https://api.dotnetniger.com/gateway/health

# Metrics
curl https://api.dotnetniger.com/gateway/metrics

# Logs
docker-compose logs -f gateway
```

---

## ⚠️ Pre-Deployment Checklist

- [ ] Tests passent (100%)
- [ ] Security scan OK
- [ ] Database backupée
- [ ] Certificat SSL en place
- [ ] Secrets en Key Vault
- [ ] Monitoring configuré
- [ ] Rollback plan prêt

---

## 📚 Documentation Complète

Pour plus de détails, consulter:
- **Déploiement complet:** [`/docs/08-DEPLOYMENT.md`](./docs/08-DEPLOYMENT.md)
- **Architecture:** [`/docs/03-ARCHITECTURE.md`](./docs/03-ARCHITECTURE.md)
- **Sécurité:** [`/SECURITY.md`](./SECURITY.md)

---

**Pour la documentation complète, voir [`/docs/08-DEPLOYMENT.md`](./docs/08-DEPLOYMENT.md)**
