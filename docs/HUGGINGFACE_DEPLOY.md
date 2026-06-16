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
| **1 port exposé** | Un Space expose un seul port (7860) |
| **Mise en veille** | Le Space s'endort après 48h d'inactivité (gratuit) |
| **Stockage éphémère** | Données perdues au redémarrage (sauf Git LFS) |
| **Pas de réseau interne** | Les Spaces ne communiquent pas entre eux |
| **Pas de domaine personnalisé** | Payant (PRO, 9$/mois) |

---

## 2. Deux Approches au Choix

### Approche A : 2 Spaces séparés (recommandé)

```
┌─────────────────────────┐    ┌─────────────────────────────┐
│  Space BACKEND (Docker)  │    │  Space FRONTEND (Docker)    │
│                          │    │                             │
│  nginx (:7860)           │    │  nginx (:7860)              │
│   ├── /identity-api/*    │    │   ├── / → fichiers WASM     │
│   ├── /api/*             │    │   ├── /api/* → proxy backend│
│   ├── /connect/*         │    │   ├── /identity-api/* → proxy│
│   ├── /Account/*         │    │   └── /connect/* → proxy    │
│   └── /.well-known/*     │    │                             │
│                          │    │  Appels API same-origin     │
│   Identity + Community   │    │  (nginx proxy → backend)    │
│   + Gateway + Identity.Web│   └─────────────────────────────┘
└─────────────────────────┘
           │                          ↑
           │  HTTPS cross-Space        │
           └──────────────────────────┘
```

**Avantages** : Build plus rapide, chaque Space est indépendant, possibilité de mettre à jour frontend sans toucher au backend.
**Inconvénient** : 2 Spaces à créer et maintenir.

### Approche B : 1 Space combiné (tout-en-un)

```
┌─────────────────────────────────────────┐
│         Space COMBINÉ (Docker)           │
│                                         │
│     nginx (:7860)                       │
│      ├── /identity-api/* → Gateway:5000 │
│      ├── /api/*          → Gateway:5000 │
│      ├── / → fichiers WASM (frontend)   │
│      └── (fallback) → Identity.Web:5100 │
│                                         │
│  ┌────────┐ ┌─────────┐ ┌───────┐ ┌───┐ │
│  │Identity│ │Community│ │Gateway│ │WASM│ │
│  │:8081   │ │:8082    │ │:5000  │ │nginx│ │
│  └────────┘ └─────────┘ └───────┘ └───┘ │
└─────────────────────────────────────────┘
```

**Avantage** : 1 seul Space, pas de CORS, pas de communication cross-Space.
**Inconvénient** : Build plus long, tout est couplé.

---

## 3. Prérequis

- Compte [Hugging Face](https://huggingface.co)
- Git + Git LFS (optionnel)
- Docker (pour tester en local)
- Projets clonés : [backend](https://github.com/akaletekoffilevis/DotnetNiger) et [frontend](https://github.com/AbdoulRaouf2005/DotnetNiger.UI)

---

## 4. Approche A : Deux Spaces séparés

### 4.1 Créer les Spaces HF

Créer 2 Spaces depuis https://huggingface.co/new-space :

| Space | SDK | Hardware | Nom suggéré |
|-------|-----|----------|-------------|
| **Backend** | Docker | Free | `dotnetniger-backend` |
| **Frontend** | Docker | Free | `dotnetniger` (principal) |

### 4.2 Déployer le Backend

```bash
# Cloner le Space backend HF
git clone https://huggingface.co/spaces/TON_COMPTE/dotnetniger-backend
cd dotnetniger-backend

# Copier le code DotnetNiger (sauf .git)
cp -r /chemin/vers/DotnetNiger/* .
rm -rf .git Dockerfile.full.hf  # pas besoin du Dockerfile combiné

# Pousser
git add . && git commit -m "Deploy backend"
git push
```

Le backend est accessible à `https://TON_COMPTE-dotnetniger-backend.hf.space`.

#### Variables secrètes backend

Dans **Settings → Repository Secrets**, ajouter :

| Secret | Exemple | Obligatoire |
|--------|---------|-------------|
| `JWT_KEY` | `MaSuperCle secrete!32caracteres` | Oui |
| `OpenIddict__Issuer` | `https://TON_COMPTE-dotnetniger-backend.hf.space/identity-api` | Oui |
| `Smtp__AppBaseUrl` | `https://TON_COMPTE-dotnetniger-backend.hf.space` | Oui |
| `Smtp__Host` | `smtp.gmail.com` | Non |
| `Smtp__Username` | `ton.email@gmail.com` | Non |
| `Smtp__Password` | `mot-de-passe-app` | Non |
| `Admin__DefaultPassword` | `Admin@123456` | Non |

> Les secrets HF sont automatiquement injectés comme variables d'environnement dans le conteneur.

### 4.3 Déployer le Frontend

```bash
# Cloner le Space frontend HF
git clone https://huggingface.co/spaces/TON_COMPTE/dotnetniger
cd dotnetniger

# Copier les fichiers de déploiement frontend
cp -r /chemin/vers/DotnetNiger.UI/* .
rm -rf .git  # garder le .git du Space HF

# Build et push
git add . && git commit -m "Deploy frontend"
git push
```

#### Build arg : BACKEND_URL

Le Dockerfile.hf a besoin de connaître l'URL du backend Space :

```dockerfile
# Dans Dockerfile.hf, le placeholder __BACKEND_URL__ est remplacé
# par sed pendant le build :
#   sed -i "s|__BACKEND_URL__|${BACKEND_URL}|g" /etc/nginx/nginx.conf
```

Pour définir cette variable sur HF :

1. Aller dans **Settings → New variable**
2. Nom : `BACKEND_URL`
3. Valeur : `https://TON_COMPTE-dotnetniger-backend.hf.space`

Puis modifier le Dockerfile.hf pour qu'il lise la variable d'env :

```dockerfile
# Utiliser une variable de build ARG (définie dans les secrets HF)
ARG BACKEND_URL
```

Ou éditer le Dockerfile.hf pour utiliser la variable d'environnement runtime :

```dockerfile
# Solution plus simple : remplacer le placeholder au démarrage
# au lieu du build
RUN echo "#!/bin/bash" > /entrypoint.sh \
    && echo "sed -i \"s|__BACKEND_URL__|${BACKEND_URL}|g\" /etc/nginx/nginx.conf" >> /entrypoint.sh \
    && echo "nginx -g 'daemon off;'" >> /entrypoint.sh \
    && chmod +x /entrypoint.sh
CMD ["/entrypoint.sh"]
```

> **Alternative Static Space** : tu peux aussi déployer le frontend comme **Static Space** (pas de Docker). Il suffit de copier le dossier `wwwroot/` publié. Mais tu perds le proxy nginx → les appels API seront cross-origin → besoin de CORS.

#### Vérifier la connexion frontend → backend

Le frontend envoie ses appels API à la même origine (grâce au proxy nginx) :
- `https://TON_COMPTE-dotnetniger.hf.space/api/v1/posts` → proxy → `https://TON_COMPTE-dotnetniger-backend.hf.space/api/v1/posts`
- `https://TON_COMPTE-dotnetniger.hf.space/identity-api/.well-known/openid-configuration` → proxy → backend

**Le navigateur voit une seule origine** → pas de problème CORS.

---

## 5. Approche B : Space Combiné (Backend + Frontend)

### 5.1 Structure

```
dotnetniger-full/
├── Dockerfile.full.hf      ← du repo backend
├── deploy/                  ← du repo backend
├── DotnetNiger.Gateway/
├── DotnetNiger.Identity/
├── DotnetNiger.Community/
├── DotnetNiger.Identity.Web/
└── frontend/                ← copie du repo UIgit
    ├── DotnetNiger.UI.csproj
    ├── Program.cs
    ├── Pages/
    ├── wwwroot/
    └── ...
```

### 5.2 Préparer le dépôt combiné

```bash
# Créer un dossier pour le Space HF
mkdir dotnetniger-full && cd dotnetniger-full
git init

# Copier le backend
cp -r /chemin/vers/DotnetNiger/* .
rm -rf .git

# Copier le frontend DANS un sous-dossier frontend/
mkdir frontend
cp -r /chemin/vers/DotnetNiger.UI/* frontend/
rm -rf frontend/.git

# Lier le Space HF
git remote add origin https://huggingface.co/spaces/TON_COMPTE/dotnetniger

# Pousser
git add . && git commit -m "Deploy combined Space"
git push
```

### 5.3 Configuration

Le `Dockerfile.full.hf` est déjà prêt :

```dockerfile
# Étapes :
# 1. Build services .NET 9 (backend)
# 2. Build Blazor WASM .NET 8 (frontend) avec API_BASE_URL=/
# 3. Image runtime : aspnet:9.0 + nginx + supervisor + frontend WASM
```

Les secrets HF sont les mêmes que pour l'approche A (section 4.2).

> **Important** : dans le `deploy/nginx.conf`, les blocs commentés pour les fichiers statiques du frontend doivent être activés. Le `Dockerfile.full.hf` copie les fichiers WASM dans `/usr/share/nginx/html` et le nginx les sert via le bloc `location /` avec `try_files`.

---

## 6. Test en Local

Avant de pousser sur HF, teste localement :

### Approche A : backend seul

```bash
cd /chemin/vers/DotnetNiger

docker build -f Dockerfile.hf -t dotnetniger-backend .
docker run -p 7860:7860 \
  -e JWT_KEY="TestKeyMin32CharactersLong!!" \
  -e OpenIddict__Issuer="http://localhost:7860/identity-api" \
  -e Smtp__AppBaseUrl="http://localhost:7860" \
  dotnetniger-backend

# Tester
curl http://localhost:7860/health
curl http://localhost:7860/identity-api/.well-known/openid-configuration
```

### Approche A : frontend seul (avec backend déjà lancé)

```bash
cd /chemin/vers/DotnetNiger.UI

docker build -f Dockerfile.hf \
  --build-arg BACKEND_URL=http://host.docker.internal:7860 \
  -t dotnetniger-frontend .
docker run -p 7861:7860 dotnetniger-frontend

# Ouvrir http://localhost:7861
```

### Approche B : combiné

```bash
cd /chemin/vers/dotnetniger-full

# Copier le Dockerfile.full.hf en Dockerfile pour HF
cp Dockerfile.full.hf Dockerfile

docker build \
  --build-arg API_BASE_URL=/ \
  -t dotnetniger-full .
docker run -p 7860:7860 \
  -e JWT_KEY="TestKeyMin32CharactersLong!!" \
  -e OpenIddict__Issuer="http://localhost:7860/identity-api" \
  -e Smtp__AppBaseUrl="http://localhost:7860" \
  dotnetniger-full

# Ouvrir http://localhost:7860
```

---

## 7. Schéma de Connexion Frontend → Backend

### Approche A (2 Spaces) : Proxy nginx

```
Navigateur                      Frontend HF                     Backend HF
    │                               │                               │
    │ GET /api/v1/posts              │                               │
    │═══════════════════►            │                               │
    │                    │           │                               │
    │                    │ proxy_pass $backend_url/api/v1/posts      │
    │                    │══════════════════════════════════════════►│
    │                    │           │                               │
    │                    │◄══════════ JSON response ════════════════│
    │◄═══════════════════│           │                               │
    │                    │           │                               │
```

**Avantage** : le navigateur voit une seule origine (`https://TON_COMPTE-dotnetniger.hf.space`) → pas de CORS. Tous les appels API sont forwardés au backend par nginx.

### Approche A (Static Space) : Cross-origin direct

```
Navigateur                      Frontend HF (Static)             Backend HF
    │                               │                               │
    │ GET https://backend/...       │                               │
    │══════════════════════════════════════════════════════════════►│
    │                               │                               │
    │◄══════════ avec CORS headers ═══════════════════════════════│
    │                               │                               │
```

**Problème** : deux origines différentes → le backend doit renvoyer des headers `Access-Control-Allow-Origin`. Avantage : pas de proxy, plus simple.

### Approche B (Combiné) : Tout sur place

```
Navigateur                      Space Combiné HF
    │                               │
    │ GET /api/v1/posts              │
    │═══════════════════►            │
    │                    │           │
    │               nginx proxy vers Gateway:5000
    │               → Identity:8081 ou Community:8082
    │                    │           │
    │◄═══════════════════│           │
    │                    │           │
    │ GET / → WASM files │           │
    │═══════════════════►            │
    │◄════════ index.html│           │
```

**Avantage** : zéro CORS, zéro configuration réseau.

---

## 8. CORS (Si Static Space ou Accès Direct)

Si tu utilises un **Static Space** pour le frontend (pas de proxy nginx), le backend doit accepter les requêtes cross-origin.

Le fichier `deploy/nginx.conf` du backend inclut déjà la configuration CORS :

```nginx
# CORS : autorise les requêtes depuis n'importe quel Space HF
map $http_origin $cors_origin {
    default "";
    "~^https://.*\.hf\.space$" $http_origin;
    "~^http://localhost" $http_origin;
}

# Réponse OPTIONS (preflight)
if ($request_method = OPTIONS) {
    add_header Access-Control-Allow-Origin $cors_origin always;
    add_header Access-Control-Allow-Methods "GET, POST, PUT, PATCH, DELETE, OPTIONS" always;
    add_header Access-Control-Allow-Headers "Authorization, Content-Type, X-API-Key, ..." always;
    add_header Access-Control-Allow-Credentials "true" always;
    return 204;
}
```

**Test CORS :**

```bash
curl -H "Origin: https://TON_COMPTE-dotnetniger.hf.space" \
  -I https://TON_COMPTE-dotnetniger-backend.hf.space/health
# Doit retourner : Access-Control-Allow-Origin: https://TON_COMPTE-dotnetniger.hf.space
```

---

## 9. Fichiers de Référence

### Backend (`DotnetNiger/`)

| Fichier | Rôle |
|---------|------|
| `Dockerfile.hf` | Build backend seul (4 services .NET + nginx + supervisor) |
| `Dockerfile.full.hf` | Build backend + frontend combiné |
| `deploy/nginx.conf` | Reverse proxy nginx avec CORS |
| `deploy/supervisord.conf` | Supervisor : démarre les 4 services + nginx |
| `deploy/start.sh` | Entrypoint du conteneur |
| `docs/HUGGINGFACE_DEPLOY.md` | Ce guide |

### Frontend (`DotnetNiger.UI/`)

| Fichier | Rôle |
|---------|------|
| `Dockerfile.hf` | Build frontend WASM + nginx proxy vers backend HF |
| `Dockerfile` | Build pour docker-compose local (nginx simple) |
| `deploy/nginx.conf` | Proxy nginx (__BACKEND_URL__ remplacé au build) |
| `nginx.conf` | Config locale (sans proxy) |

---

## 10. Résumé des URLs Après Déploiement

### Approche A (2 Spaces)

| Service | URL |
|---------|-----|
| Frontend (Space principal) | `https://TON_COMPTE-dotnetniger.hf.space` |
| Backend Space | `https://TON_COMPTE-dotnetniger-backend.hf.space` |
| Swagger | `https://TON_COMPTE-dotnetniger.hf.space/swagger` |
| Health | `https://TON_COMPTE-dotnetniger.hf.space/health` |
| OIDC Discovery | `https://TON_COMPTE-dotnetniger.hf.space/identity-api/.well-known/openid-configuration` |

### Approche B (1 Space combiné)

| Service | URL |
|---------|-----|
| App (frontend + backend) | `https://TON_COMPTE-dotnetniger.hf.space` |
| Swagger | `https://TON_COMPTE-dotnetniger.hf.space/swagger` |
| Health | `https://TON_COMPTE-dotnetniger.hf.space/health` |
| Developer Portal | `https://TON_COMPTE-dotnetniger.hf.space/Developer/Dashboard` |

---

## 11. Anti-Sommeil (Keep Alive)

Les Spaces gratuits s'endorment après 48h d'inactivité. Ajoute ce workflow GitHub :

```yaml
# .github/workflows/ping-hf.yml (dans le repo backend)
name: Keep HF Space alive
on:
  schedule:
    - cron: '*/25 * * * *'  # toutes les 25 minutes
jobs:
  ping:
    runs-on: ubuntu-latest
    steps:
      - run: curl -s https://TON_COMPTE-dotnetniger.hf.space/health
      - run: curl -s https://TON_COMPTE-dotnetniger-backend.hf.space/health
```

---

## 12. Commandes Rapides

```bash
# Backend seul
docker build -f Dockerfile.hf -t dotnetniger-backend .

# Frontend seul (avec proxy vers backend)
docker build -f Dockerfile.hf \
  --build-arg BACKEND_URL=https://TON_COMPTE-dotnetniger-backend.hf.space \
  -t dotnetniger-frontend .

# Combiné
docker build -f Dockerfile.full.hf \
  --build-arg API_BASE_URL=/ \
  -t dotnetniger-full .

# Push vers HF
git add . && git commit -m "update" && git push
```
