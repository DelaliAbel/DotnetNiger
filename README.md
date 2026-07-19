# DotnetNiger

Plateforme communautaire pour les développeurs .NET au Niger — **backend monolithique** (4 projets) + **frontend Blazor WASM**.

## Architecture actuelle

```
┌─────────────────────────────────────────────────────────────┐
│                    DotnetNiger.Client                         │
│                    (Blazor WASM, net8.0)                     │
│                         │                                    │
│                         ▼ HTTPS                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              DotnetNiger.Server                       │    │
│  │              (ASP.NET Core API, net9.0)              │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │    │
│  │  │ Controllers │  │  Services   │  │  OpenIddict │   │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘   │    │
│  └─────────────────────────────────────────────────────┘    │
│                         │                                    │
│                         ▼ EF Core                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │          DotnetNiger.Infrastructure                   │    │
│  │        (Services, Data, EF Core, net9.0)             │    │
│  └─────────────────────────────────────────────────────┘    │
│                         │                                    │
│                         ▼                                     │
│  ┌─────────────────────────────────────────────────────┐    │
│  │            DotnetNiger.Domain                         │    │
│  │         (Entities, DTOs, Constants, net9.0)          │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
              ┌─────────────────┐
              │   SQL Server    │
              │  (Database)     │
              └─────────────────┘
```

### Projets

| Projet | Rôle | Framework |
|--------|------|-----------|
| **DotnetNiger.Domain** | Entités, DTOs, Constantes, Enums | net9.0 |
| **DotnetNiger.Infrastructure** | Services métier, EF Core, DbContext, OpenIddict | net9.0 |
| **DotnetNiger.Server** | API Controllers, Auth, OpenIddict Server | net9.0 |
| **DotnetNiger.Client** | Blazor WebAssembly Frontend | net8.0 |

> **Note** : L'ancienne architecture multi-repo (Identity, Community, Gateway, Identity.Web) a été fusionnée en ce monolithe unique.

---

## Stack technique

- **.NET** : 9.0 (backend) / 8.0 (Blazor WASM)
- **Auth** : OpenIddict 5.8 (OAuth2 / OIDC), JWT + Refresh tokens, Password Grant
- **DB** : SQL Server + Entity Framework Core
- **Frontend** : Blazor WebAssembly, MudBlazor, Chart.js
- **CI/CD** : GitHub Actions (build + deploy vers branches `deploy/backend` et `deploy/frontend`)
- **Déploiement** : MonsterASP / RunASP.net

---

## API Endpoints principaux

Base URL : `https://api.dotnetniger.com/api/v{version}` (via Gateway) ou `http://localhost:5000/api/v{version}` (local)

### Auth — `/api/v1/auth`

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/connect/token` | Token OAuth2 (password, refresh_token, client_credentials) |
| POST | `/register` | Inscription utilisateur (envoi code confirmation) |
| POST | `/confirm-email` | Confirmation email par code |
| GET | `/confirm-email` | Confirmation email par lien (API) |
| GET | `/Account/ConfirmEmail` | Page dédiée confirmation email (Razor Pages) |
| POST | `/resend-code` | Renvoyer code confirmation |
| POST | `/login` | Connexion email/mot de passe |
| POST | `/forgot-password` | Demande réinitialisation |
| POST | `/reset-password` | Réinitialisation |
| POST | `/refresh` | Rafraîchir access token |
| POST | `/logout` | Déconnexion (revoke refresh token) |
| GET | `/userinfo` | Infos utilisateur courant (rôles, permissions) |
| GET | `/external-login` | Redirection fournisseur externe (Google, GitHub, Microsoft) |
| GET | `/external-callback` | Callback OAuth externe (server-side) |
| GET | `/external-callback-frontend` | Callback OAuth externe (Blazor) |
| POST | `/bootstrap-web-ui` | Créer le client OIDC "web-ui" |
| POST | `/verify-2fa` | Vérification 2FA |
| POST | `/verify-2fa-recovery` | Vérification 2FA (code récupération) |
| POST | `/change-email` | Demande changement email |
| POST | `/confirm-change-email` | Confirmation changement email |
| POST | `/change-password` | Changement mot de passe |

### Profile — `/api/v1/profile`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Profil courant |
| PUT | `/` | Modifier profil |
| DELETE | `/` | Supprimer compte |
| GET | `/two-factor/status` | Statut 2FA |
| POST | `/two-factor/setup` | Initialiser 2FA |
| POST | `/two-factor/enable` | Activer 2FA |
| POST | `/two-factor/disable` | Désactiver 2FA |
| POST | `/two-factor/recovery-codes` | Nouveaux codes récupération |
| GET | `/login-history` | Historique connexions |

### Admin — `/api/v1/admin` (Admin/SuperAdmin)

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/invite` | Inviter admin par email |
| GET | `/stats` | Statistiques système (users, roles, permissions, API keys, services, clients) |
| GET | `/stats/mine` | **Stats personnelles** (Collaborator+) : mes events, blogs, ressources, projets |
| GET | `/login-history` | Historique connexions (paginé) |
| GET | `/audit-logs` | Journal d'audit (filtres, paginé) |
| GET | `/users` | Tous les utilisateurs |
| GET | `/users/{id}` | Utilisateur par ID |
| PATCH | `/users/{id}/status` | Activer/désactiver utilisateur |
| PATCH | `/users/{id}/profile` | Modifier profil utilisateur |
| POST | `/users/{id}/roles` | Assigner rôle (remplace existants) |
| DELETE | `/users/{id}/roles/{roleName}` | Retirer rôle |
| DELETE | `/users/{id}` | Supprimer utilisateur |
| POST | `/users` | Créer utilisateur (admin) |

### Posts — `/api/v1/posts`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister articles (filtres: published, category, tag, query, page, pageSize) |
| GET | `/{id}` | Article par ID |
| GET | `/{slug}` | Article par slug |
| GET | `/by-slug/{slug}` | OG meta pour article |
| GET | `/mine` | Mes articles (auth) |
| POST | `/` | Créer article |
| PUT | `/{id}` | Modifier article |
| PATCH | `/{id}/publish` | Publier article |
| PATCH | `/{id}/unpublish` | Dépublier article |
| POST | `/{id}/views` | Incrémenter vues |
| DELETE | `/{id}` | Supprimer article |

### Events — `/api/v1/events`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister événements (filtres: published, past, eventType, query, tag, dates) |
| GET | `/mine` | Mes événements (auth) |
| GET | `/upcoming` | Événements à venir |
| GET | `/{id}` | Événement par ID |
| GET | `/{slug}` | Événement par slug |
| GET | `/by-slug/{slug}` | OG meta pour événement |
| POST | `/` | Créer événement |
| PUT | `/{id}` | Modifier événement |
| DELETE | `/{id}` | Supprimer événement |
| POST | `/registrations` | S'inscrire à un événement |
| DELETE | `/{eventId}/registrations` | Annuler inscription |
| GET | `/{eventId}/registrations` | Inscriptions à un événement |
| GET | `/pending` | Événements en attente (Admin) |
| PATCH | `/{id}/approve` | Approuver événement (Admin) |
| PATCH | `/{id}/reject` | Rejeter événement (Admin) |

### Comments — `/api/v1/comments`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/post/{postId}` | Commentaires d'un article |
| GET | `/event/{eventId}` | Commentaires d'un événement |
| GET | `/{id}` | Commentaire par ID |
| POST | `/` | Créer commentaire |
| PUT | `/{id}` | Modifier commentaire |
| DELETE | `/{id}` | Supprimer commentaire |

### Resources — `/api/v1/resources`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister ressources (filtres: resourceType, level, query, tag, categoryId, createdBy) |
| GET | `/mine` | Mes ressources (auth) |
| GET | `/{id}` | Ressource par ID |
| GET | `/{slug}` | Ressource par slug |
| GET | `/by-slug/{slug}` | OG meta pour ressource |
| GET | `/types` | Types de ressources distincts |
| GET | `/levels` | Niveaux distincts |
| POST | `/` | Créer ressource |
| PUT | `/{id}` | Modifier ressource |
| DELETE | `/{id}` | Supprimer ressource |
| POST | `/{id}/views` | Incrémenter vues |

### Projects — `/api/v1/projects`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister projets (filtres: status, query) |
| GET | `/featured` | Projets à la une |
| GET | `/{id}` | Projet par ID |
| GET | `/slug/{slug}` | Projet par slug |
| POST | `/` | Créer projet |
| PUT | `/{id}` | Modifier projet |
| DELETE | `/{id}` | Supprimer projet |

### Categories — `/api/v1/categories`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister catégories |
| GET | `/{id}` | Catégorie par ID |
| GET | `/{slug}` | Catégorie par slug |
| POST | `/` | Créer catégorie (Admin) |
| PUT | `/{id}` | Modifier catégorie (Admin) |
| DELETE | `/{id}` | Supprimer catégorie (Admin) |

### Tags — `/api/v1/tags`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister tags |
| GET | `/{id}` | Tag par ID |
| GET | `/{slug}` | Tag par slug |
| POST | `/` | Créer tag (Admin) |
| PUT | `/{id}` | Modifier tag (Admin) |
| DELETE | `/{id}` | Supprimer tag (Admin) |

### Members — `/api/v1/members`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Lister membres (filtres: query, country, page, pageSize) |
| GET | `/team` | Membres équipe (IsTeamMember=true) |
| GET | `/{id}` | Membre par ID |
| POST | `/` | Créer profil membre (auth) |
| PUT | `/{id}` | Modifier profil membre (auth) |
| DELETE | `/{id}` | Supprimer profil membre (auth) |

### Partners — `/api/v1/partners`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Partenaires actifs (filtre: partnerType) |
| GET | `/{id}` | Partenaire par ID |
| POST | `/` | Créer partenaire (Admin) |
| PUT | `/{id}` | Modifier partenaire (Admin) |
| DELETE | `/{id}` | Supprimer partenaire (Admin) |

### Upload — `/api/v1/upload`

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/` | Upload image (fichier, max 2MB) |
| POST | `/base64` | Upload image (base64) |
| DELETE | `/` | Supprimer fichier uploadé |

### Newsletter — `/api/v1/newsletter`

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/subscribe` | S'abonner |
| POST | `/unsubscribe` | Se désabonner |
| GET | `/` | Abonnements (Admin) |
| GET | `/count` | Nombre abonnés actifs |
| DELETE | `/{email}` | Supprimer abonné par email (Admin) |

### Contact — `/api/v1/contact`

| Méthode | Route | Description |
|---------|-------|-------------|
| POST | `/` | Envoyer message contact |

### Notifications — `/api/v1/notifications`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/{userId}` | Notifications d'un utilisateur |
| GET | `/{userId}/unread-count` | Nombre non lues |
| POST | `/{userId}` | Envoyer notification |
| PATCH | `/{userId}/{id}/read` | Marquer comme lue |
| PATCH | `/{userId}/read-all` | Tout marquer comme lu |

### Stats — `/api/v1/stats`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Statistiques plateforme |

### Search — `/api/v1/search`

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Recherche multi-contenus (posts, events, resources, projects) |

---

## Base de données

- **Hébergement** : databaseasp.net (`db57026`) / SQL Server local (dev)
- **Migrations** : Appliquées via `DotnetNiger.DbManager`
  ```bash
  dotnet run --project DotnetNiger.DbManager
  ```
  Ce projet applique les migrations Identity + Community sur la même base, puis seed le tenant, les rôles, permissions et le compte SuperAdmin.

---

## Développement local

### Prérequis
- .NET 9 SDK (backend) + .NET 8 SDK (frontend)
- SQL Server (LocalDB, Docker, ou instance distante)
- Visual Studio 2022 / Rider / VS Code

### Lancer le backend
```bash
cd DotnetNiger.Server
dotnet run --urls http://localhost:5000
```

### Lancer le frontend
```bash
cd DotnetNiger.Client
dotnet run --urls http://localhost:5201
```

### Docker (SQL Server seulement)
```bash
docker compose up -d
```
Lance uniquement SQL Server (port 1433). Les services .NET tournent en local hors Docker.

---

## Déploiement production

Le déploiement utilise le workflow GitHub Actions **deploy.yml** sur la branche `BackEnd` :

1. **CI** (`.github/workflows/ci.yml`) : `dotnet restore` + `dotnet build` (Release, warnings as errors) sur `DotnetNiger.sln`
2. **Deploy** (`.github/workflows/deploy.yml`) :
   - Job `build-backend` : `dotnet publish DotnetNiger.Server` → push vers branche `deploy/backend`
   - Job `build-frontend` : `dotnet publish DotnetNiger.Client` → push vers branche `deploy/frontend`

Chaque branche `deploy/*` contient les binaires pré-compilés. Le serveur (MonsterASP / RunASP.net) est configuré pour déployer automatiquement depuis ces branches.

### Configuration production

Les `appsettings.Production.json` sont **gitignorés**. Configurer le serveur avec :
- Variables d'environnement pour les secrets (JWT, SMTP, OAuth, connexions DB)
- Ou déposer un `appsettings.Production.json` sur le serveur après le premier déploiement

---

## Rôles & Permissions

| Rôle | Description | Permissions clés |
|------|-------------|------------------|
| **SuperAdmin** | Accès total (Identity + Community) | Toutes permissions, gestion tenants, roles, permissions, API keys |
| **Admin** | Administration Community | Gestion users, events, posts, resources, projects, categories, tags |
| **Collaborator** | Contributeur validé | CRUD propres events/posts/resources/projects, dashboard perso |
| **User** | Utilisateur standard | Lecture publique, profil, commentaires, inscriptions events |
| **Client** | Client OAuth2 (machine) | Selon scopes octroyés |

> **Règle métier** : Un compte = **UN SEUL RÔLE**. L'assignation d'un nouveau rôle remplace l'ancien.

---

## Licence

Propriété de DotnetNiger Community. Tous droits réservés.