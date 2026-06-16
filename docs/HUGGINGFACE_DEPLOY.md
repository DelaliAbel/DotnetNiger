# Guide de Déploiement — Hugging Face Spaces

> Ce guide explique comment déployer DotnetNiger (backend + frontend) sur **Hugging Face Spaces** gratuitement.

---

## 1. Pourquoi Hugging Face ?

| Avantage | Détail |
|----------|--------|
| **Gratuit** | 2 vCPU, 16 GB RAM, 50 GB disque |
| **Docker** | Support Docker natif — idéal pour .NET |
| **Static** | Hébergement de site statique gratuit |
| **HTTPS** | Automatique, domaine `*.hf.space` |
| **Secrets** | Variables d'environnement sécurisées |

### Limitations

| Limite | Détail |
|--------|--------|
| **1 port exposé** | Un Space Docker expose un seul port (7860) |
| **Mise en veille** | Le Space s'endort après 48h d'inactivité (gratuit) |
| **Stockage éphémère** | Les fichiers sont perdus au redémarrage (sauf si commit) |
| **Pas de réseau interne** | Les Spaces ne communiquent pas entre eux |
| **Pas de domaine personnalisé** | Payant (PRO, à partir de 9$/mois) |

---

## 2. Architecture sur HF Spaces

```
                     ┌──────────────────────────────┐
                     │     Docker Space (1 seul)     │
                     │                              │
                     │  nginx (port 7860 exposé)     │
                     │    ├── /identity-api/* → Identity:8081
                     │    ├── /api/*          → Community:8082
                     │    ├── /swagger        → Gateway:5000
                     │    └── /               → Frontend WASM
                     │                              │
                     │  ┌──────┐ ┌────────┐ ┌─────┐ │
                     │  │Identity││Community││Gateway│ │
                     │  │:8081   ││:8082    ││:5000  │ │
                     │  └──────┘ └────────┘ └─────┘ │
                     └──────────────────────────────┘
```

Tous les services tournent **dans un seul conteneur** (Hugging Face expose 1 seul port). Un reverse proxy nginx route les requêtes vers le bon service interne.

---

## 3. Prérequis

- Compte [Hugging Face](https://huggingface.co)
- Git + Git LFS
- Docker (pour tester en local)
- Projet DotnetNiger cloné

---

## 4. Créer le Space

### 4.1 Via l'interface web

1. Aller sur https://huggingface.co
2. Cliquer sur ton avatar → **New Space**
3. Configurer :
   - **Space Name** : `dotnetniger` (ou autre)
   - **License** : MIT
   - **Space SDK** : **Docker**
   - **Docker Template** : **Blank**
   - **Space Hardware** : **Free** (2 vCPU, 16 GB)
4. **Create Space**

### 4.2 Cloner le Space

```bash
# Cloner le Space HF
git clone https://huggingface.co/spaces/TON_COMPTE/dotnetniger
cd dotnetniger

# Copier les fichiers de déploiement DotnetNiger
cp -r /chemin/vers/DotnetNiger/* .
```

---

## 5. Fichiers de Déploiement

### 5.1 `Dockerfile`

```dockerfile
# === Étape 1 : Build des services .NET ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier les fichiers projet
COPY DotnetNiger.Gateway/DotnetNiger.Gateway.csproj DotnetNiger.Gateway/
COPY DotnetNiger.Identity/DotnetNiger.Identity.csproj DotnetNiger.Identity/
COPY DotnetNiger.Community/DotnetNiger.Community.csproj DotnetNiger.Community/
COPY DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj DotnetNiger.Identity.Web/
COPY DotnetNiger.slnx .

# Restore
RUN dotnet restore

# Copier tout le code source
COPY . .

# Publier chaque service
RUN dotnet publish DotnetNiger.Identity/DotnetNiger.Identity.csproj \
    -c Release -o /app/identity /p:UseAppHost=false
RUN dotnet publish DotnetNiger.Community/DotnetNiger.Community.csproj \
    -c Release -o /app/community /p:UseAppHost=false
RUN dotnet publish DotnetNiger.Gateway/DotnetNiger.Gateway.csproj \
    -c Release -o /app/gateway /p:UseAppHost=false
RUN dotnet publish DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj \
    -c Release -o /app/identity-web /p:UseAppHost=false

# === Étape 2 : Image runtime avec nginx + aspnet ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Installer nginx et supervisor
RUN apt-get update && apt-get install -y \
    nginx \
    supervisor \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copier les binaires
COPY --from=build /app/identity ./identity
COPY --from=build /app/community ./community
COPY --from=build /app/gateway ./gateway
COPY --from=build /app/identity-web ./identity-web

# Créer les dossiers de logs
RUN mkdir -p /app/logs /var/log/supervisor

# Copier la config nginx
COPY deploy/nginx.conf /etc/nginx/nginx.conf

# Copier la config supervisor
COPY deploy/supervisord.conf /etc/supervisor/conf.d/supervisord.conf

# Copier le script de démarrage
COPY deploy/start.sh /start.sh
RUN chmod +x /start.sh

# Créer les dossiers de données SQLite
RUN mkdir -p /app/data

# Port exposé pour HF (7860 est le port par défaut HF)
EXPOSE 7860

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:7860/health || exit 1

CMD ["/start.sh"]
```

### 5.2 `deploy/nginx.conf`

```nginx
events {
    worker_connections 1024;
}

http {
    include mime.types;
    default_type application/octet-stream;

    # Buffer plus grands pour OIDC
    proxy_buffer_size 128k;
    proxy_buffers 4 256k;
    proxy_busy_buffers_size 256k;

    # Timeouts
    proxy_connect_timeout 30s;
    proxy_read_timeout 60s;
    proxy_send_timeout 60s;

    # Gzip
    gzip on;
    gzip_types text/css application/javascript application/json image/svg+xml;
    gzip_min_length 256;

    server {
        listen 7860;

        # === GATEWAY ===
        location /health {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /swagger {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /metrics {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # === IDENTITY (via Gateway) ===
        location /identity-api/ {
            proxy_pass http://127.0.0.1:5000/identity-api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /connect/ {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /Account/ {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /.well-known/ {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # === COMMUNITY (via Gateway) ===
        location /api/ {
            proxy_pass http://127.0.0.1:5000;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # === FALLBACK : tout le reste → Identity.Web ===
        location / {
            proxy_pass http://127.0.0.1:5100;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;

            # Buffering pour le rendu Razor Pages
            proxy_buffering on;
        }
    }
}
```

### 5.3 `deploy/supervisord.conf`

```ini
[supervisord]
nodaemon=true
user=root
logfile=/var/log/supervisor/supervisord.log
pidfile=/var/run/supervisord.pid

[program:identity]
command=dotnet /app/identity/DotnetNiger.Identity.dll
directory=/app/identity
environment=ASPNETCORE_URLS="http://+:8081",ASPNETCORE_ENVIRONMENT="Production"
autostart=true
autorestart=true
startretries=3
stderr_logfile=/app/logs/identity.err.log
stdout_logfile=/app/logs/identity.out.log

[program:community]
command=dotnet /app/community/DotnetNiger.Community.dll
directory=/app/community
environment=ASPNETCORE_URLS="http://+:8082",ASPNETCORE_ENVIRONMENT="Production"
autostart=true
autorestart=true
startretries=3
stderr_logfile=/app/logs/community.err.log
stdout_logfile=/app/logs/community.out.log

[program:gateway]
command=dotnet /app/gateway/DotnetNiger.Gateway.dll
directory=/app/gateway
environment=ASPNETCORE_URLS="http://+:5000",ASPNETCORE_ENVIRONMENT="Production"
autostart=true
autorestart=true
startretries=3
stderr_logfile=/app/logs/gateway.err.log
stdout_logfile=/app/logs/gateway.out.log

[program:identity-web]
command=dotnet /app/identity-web/DotnetNiger.Identity.Web.dll
directory=/app/identity-web
environment=ASPNETCORE_URLS="http://+:5100",ASPNETCORE_ENVIRONMENT="Production"
autostart=true
autorestart=true
startretries=3
stderr_logfile=/app/logs/identity-web.err.log
stdout_logfile=/app/logs/identity-web.out.log

[program:nginx]
command=nginx -g "daemon off;"
autostart=true
autorestart=true
stderr_logfile=/var/log/nginx/error.log
stdout_logfile=/var/log/nginx/access.log
```

### 5.4 `deploy/start.sh`

```bash
#!/bin/bash
set -e

# Appliquer les variables d'environnement HF aux appsettings
# HF expose les secrets comme variables d'environnement
# On les écrit dans un fichier .env que les services lisent

# Démarrer supervisor (qui lance nginx + tous les services)
exec supervisord -c /etc/supervisor/conf.d/supervisord.conf
```

### 5.5 Structure finale du Space

```
dotnetniger-hf-space/
├── .gitignore
├── Dockerfile
├── deploy/
│   ├── nginx.conf
│   ├── supervisord.conf
│   └── start.sh
├── DotnetNiger.Gateway/        ← symlink ou copie
├── DotnetNiger.Identity/
├── DotnetNiger.Community/
├── DotnetNiger.Identity.Web/
├── DotnetNiger.slnx
└── README.md
```

---

## 6. Configuration via Secrets HF

Dans **Settings → Repository Secrets** du Space, ajouter :

| Secret | Valeur | Obligatoire |
|--------|--------|-------------|
| `JWT_KEY` | Ta clé JWT (min 32 caractères) | **Oui** |
| `OpenIddict__Issuer` | `https://ton-compte-dotnetniger.hf.space/identity-api` | Oui |
| `Smtp__AppBaseUrl` | `https://ton-compte-dotnetniger.hf.space` | Oui |
| `Smtp__Host` | `smtp.gmail.com` | Non |
| `Smtp__Username` | ton.email@gmail.com | Non |
| `Smtp__Password` | mot de passe d'application | Non |
| `Smtp__FromEmail` | `noreply@dotnetniger.com` | Non |
| `Gateway__RegistrationKey` | Une clé secrète | Non |
| `InternalApiKey` | Une clé secrète | Non |
| `Admin__DefaultPassword` | `Admin@123456` | Non |

> Les variables d'environnement sont automatiquement disponibles dans le conteneur. Pas besoin de fichier `.env`.

---

## 7. Déploiement

### 7.1 Premier déploiement

```bash
# Depuis le dossier du Space
git add .
git commit -m "Initial HF Space deployment"
git push

# Hugging Face build automatiquement l'image Docker
# et déploie le Space
```

### 7.2 Mise à jour

```bash
# Après des changements dans le code DotnetNiger
git add .
git commit -m "Update DotnetNiger to version X"
git push
```

Le build prend **5-15 minutes** (download NuGet + build .NET + Docker).

---

## 8. Accès à l'Application

Après déploiement, l'application est accessible à :

```
https://TON_COMPTE-dotnetniger.hf.space
```

| URL | Description |
|-----|-------------|
| `https://...hf.space` | Developer Portal (Identity.Web) |
| `https://...hf.space/swagger` | Swagger UI |
| `https://...hf.space/health` | Health check |
| `https://...hf.space/identity-api/.well-known/openid-configuration` | OIDC Discovery |
| `https://...hf.space/identity-api/api/v1/diagnostics/health` | Identity health |

---

## 9. Déploiement du Frontend Blazor (Space Static séparé)

### Alternative : 2 Spaces au lieu d'un

Si tu préfères, le frontend Blazor WASM peut être dans un **Static Space** séparé.

#### Créer le Static Space

1. Aller sur https://huggingface.co → **New Space**
2. **Space Name** : `dotnetniger-ui`
3. **Space SDK** : **Static**
4. **Create Space**

#### Publier le frontend

```bash
# Dans le repo UIgit
dotnet publish -c Release -o publish/frontend

# Cloner le Static Space
git clone https://huggingface.co/spaces/TON_COMPTE/dotnetniger-ui
cd dotnetniger-ui

# Copier les fichiers statiques
cp -r ../publish/frontend/wwwroot/* .

# Modifier appsettings.json avec l'URL du backend HF
# wwwroot/appsettings.json → "ApiBaseUrl": "https://TON_COMPTE-dotnetniger.hf.space"

git add .
git commit -m "Deploy frontend"
git push
```

Le frontend est accessible à `https://TON_COMPTE-dotnetniger-ui.hf.space`.

> **Important** : le frontend Blazor WASM doit être compilé avec l'URL du backend HF dans `appsettings.json`. Voir la section Dockerfile du frontend (`API_BASE_URL` build arg).

---

## 10. Persistance SQLite

Hugging Face Spaces ont un **stockage éphémère**. Les bases SQLite sont perdues à chaque redémarrage (déploiement, mise en veille, etc.).

### Solutions

| Solution | Description |
|----------|-------------|
| **Seed automatique** | Le DbContext crée et seed la base au premier lancement (déjà en place) |
| **Git LFS** | Stocker les .db dans Git LFS (limité à la taille du repo) |
| **Base externe** | Utiliser une DBaaS gratuite (Turso, Neon, Supabase) |

### Seed automatique

Par défaut, Identity se configure pour recréer la base si elle n'existe pas, et le super admin est re-seed :

```csharp
// Déjà implémenté dans Identity
if (!await context.Users.AnyAsync())
{
    await SeedSuperAdminAsync(context);
}
```

Les données sont réinitialisées à chaque redémarrage. Pour la démo, ça suffit.

### Base SQLite persistée avec Git LFS

```bash
# Dans le Space
git lfs track "*.db"
git add .gitattributes
git commit -m "Track SQLite with LFS"
```

Les bases sont alors commitées dans Git LFS et survivent aux redémarrages. Mais attention à la taille.

---

## 11. Mise en veille (Free Tier)

Les Spaces gratuits s'endorment après **48 heures d'inactivité**.

### Pour éviter la mise en veille

- Configurer un **cron** (GitHub Actions, UptimeRobot) qui ping le Space toutes les 30 minutes :

```yaml
# .github/workflows/ping.yml (dans le repo GitHub)
name: Keep HF Space alive
on:
  schedule:
    - cron: '*/30 * * * *'
jobs:
  ping:
    runs-on: ubuntu-latest
    steps:
      - run: curl -s https://TON_COMPTE-dotnetniger.hf.space/health
```

- Passer en **PRO** (9$/mois) : pas de mise en veille, domaine personnalisé.

---

## 12. Debug & Logs

### Voir les logs

```bash
# Interface HF
Aller sur ton Space → **Logs** → onglet **Docker Logs**
```

### Accéder au conteneur en SSH

HF Spaces ne permettent pas de SSH directement. Utilise les logs pour le debug :

```dockerfile
# Dans le Dockerfile, ajouter une sortie de logs verbeuse
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
ENV Logging__LogLevel__Default=Information
```

### Redémarrer le Space

Settings → **Restart Space** ou `git commit --allow-empty -m "restart" && git push`.

---

## 13. Exemple Complet : `Dockerfile` optimisé

```dockerfile
# === BUILD ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore DotnetNiger.slnx \
    && dotnet publish DotnetNiger.Identity/DotnetNiger.Identity.csproj -c Release -o /app/identity /p:UseAppHost=false \
    && dotnet publish DotnetNiger.Community/DotnetNiger.Community.csproj -c Release -o /app/community /p:UseAppHost=false \
    && dotnet publish DotnetNiger.Gateway/DotnetNiger.Gateway.csproj -c Release -o /app/gateway /p:UseAppHost=false \
    && dotnet publish DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj -c Release -o /app/identity-web /p:UseAppHost=false

# === RUNTIME ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN apt-get update && apt-get install -y nginx supervisor curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/identity ./identity
COPY --from=build /app/community ./community
COPY --from=build /app/gateway ./gateway
COPY --from=build /app/identity-web ./identity-web
COPY deploy/ /app/deploy/

RUN cp /app/deploy/nginx.conf /etc/nginx/nginx.conf \
    && cp /app/deploy/supervisord.conf /etc/supervisor/conf.d/supervisord.conf \
    && mkdir -p /app/data /app/logs /var/log/supervisor

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 7860

CMD ["/app/deploy/start.sh"]
```

---

## 14. Commandes Utiles

```bash
# Tester en local (comme sur HF)
docker build -t dotnetniger-hf .
docker run -p 7860:7860 \
  -e JWT_KEY="TestKeyMin32CharactersLong!!" \
  -e OpenIddict__Issuer="http://localhost:7860/identity-api" \
  -e Smtp__AppBaseUrl="http://localhost:7860" \
  dotnetniger-hf

# Ouvrir http://localhost:7860

# Voir les logs en local
docker logs -f dotnetniger-hf
```

---

## 15. Comparaison : HF Spaces vs Oracle Cloud

| Critère | Hugging Face (Free) | Oracle Cloud (Always Free) |
|---------|---------------------|---------------------------|
| **Prix** | Gratuit | Gratuit |
| **RAM/CPU** | 16 GB, 2 vCPU | 1-4 GB, 1-3 OCPU |
| **Stockage** | 50 GB (éphémère) | 200 GB (persistant) |
| **Docker** | Oui (1 conteneur) | Oui (Docker Compose) |
| **Services** | Tout dans 1 conteneur | Chacun son conteneur |
| **Mise en veille** | Oui (48h) | Non |
| **Domaine** | `*.hf.space` (HTTPS) | IP publique (HTTPS via Cloudflare) |
| **Persistance** | Éphémère (sauf Git LFS) | Volume persistant |
| **Setup** | Très simple | Moyen (créer VM, installer Docker) |
| **Idéal pour** | Démo, POC, test | Production continue |

> **Recommandation** : utilise Hugging Face pour une **démo rapide** ou un **POC**. Passe sur **Oracle Cloud** (ou un VPS à 5€/mois) pour une **vraie production** avec données persistantes et pas de mise en veille.
