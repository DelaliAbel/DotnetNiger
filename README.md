# DotnetNiger

Plateforme communautaire pour les développeurs .NET au Niger — backend monolithique découpé en 4 projets.

## Architecture

```
Client (Netlify)
  │
  ▼
Gateway (dotnetniger.runasp.net:5000)
  │
  ├──► Identity API (identity-dotnetniger.runasp.net:5075)
  │     └──► Identity.Web (Developer Portal :5100)
  │
  └──► Community API (community-dotnetniger.runasp.net:5050)
```

## Projets

| Projet | Rôle | URL Prod |
|---|---|---|
| **DotnetNiger.Identity** | Serveur OAuth2/OIDC (OpenIddict), auth multi-tenant, RBAC, gestion des utilisateurs/tenants/clients OAuth2/clés API | `https://identity-dotnetniger.runasp.net` |
| **DotnetNiger.Identity.Web** | Portail développeur (Razor Pages), authentifié via OIDC | — |
| **DotnetNiger.Community** | API communautaire : blog, événements, commentaires, membres, projets, ressources, etc. | `https://community-dotnetniger.runasp.net` |
| **DotnetNiger.Gateway** | API Gateway (Ocelot), point d'entrée unique, rate limiting, caching, QoS | `https://dotnetniger.runasp.net` |

## Stack technique

- .NET 9 (net9.0)
- ASP.NET Core Minimal / MVC / Razor Pages
- OpenIddict 5.8.0 (OAuth2 / OpenID Connect)
- Ocelot (API Gateway)
- Entity Framework Core + SQL Server
- Blazor WASM (Frontend séparé)
- GitHub Actions (CI)

---

## DotnetNiger.Identity — API Endpoints

Base: `https://identity-dotnetniger.runasp.net/api/v{version}` (via Gateway: `https://dotnetniger.runasp.net/api/{...}`)

### Auth — `/api/v{version}/auth`

| Méthode | Route | Description |
|---|---|---|
| POST | `/connect/token` | Token OAuth2 (password, client_credentials, refresh_token, authorization_code, external_login) |
| POST | `register-tenant` | Inscription multi-tenant (tenant + admin + client OAuth2 + clé API) |
| POST | `login` | Connexion email/mot de passe |
| POST | `register` | Inscription utilisateur (envoi code confirmation) |
| POST | `confirm-email` | Confirmation email par code |
| GET | `confirm-email` | Confirmation email par lien |
| POST | `resend-code` | Renvoyer code confirmation |
| POST | `verify-2fa` | Vérification 2FA |
| POST | `verify-2fa-recovery` | Vérification 2FA (code récupération) |
| POST | `logout` | Déconnexion |
| GET | `external-login` | Redirection fournisseur externe (Google, GitHub, Microsoft) |
| GET | `external-callback` | Callback OAuth externe (server-side) |
| GET | `external-callback-frontend` | Callback OAuth externe (frontend/Blazor) |
| GET | `userinfo` | Infos utilisateur courant |
| POST | `bootstrap-web-ui` | Créer le client OIDC "web-ui" |
| POST | `forgot-password` | Demande réinitialisation mot de passe |
| POST | `reset-password` | Réinitialisation mot de passe |
| POST | `refresh` | Rafraîchir token |

### Profile — `/api/v{version}/profile`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Profil courant |
| PUT | `/` | Modifier profil |
| DELETE | `/` | Supprimer compte |
| GET | `two-factor/status` | Statut 2FA |
| POST | `two-factor/setup` | Initialiser 2FA |
| POST | `two-factor/enable` | Activer 2FA |
| POST | `two-factor/disable` | Désactiver 2FA |
| POST | `two-factor/recovery-codes` | Nouveaux codes récupération |
| GET | `login-history` | Historique connexions |
| POST | `change-email` | Demander changement email |
| POST | `confirm-change-email` | Confirmer changement email |
| POST | `change-password` | Changer mot de passe |

### Admin — `/api/v{version}/admin`

| Méthode | Route | Description |
|---|---|---|
| POST | `invite` | Inviter admin par email |
| GET | `stats` | Statistiques système |
| GET | `audit-logs` | Journal d'audit |
| GET | `users` | Tous les utilisateurs |
| GET | `users/{id}` | Utilisateur par ID |
| PATCH | `users/{id}/status` | Activer/désactiver utilisateur |
| POST | `users/{id}/roles` | Assigner rôle |
| DELETE | `users/{id}` | Supprimer utilisateur |
| POST | `users` | Créer utilisateur |
| GET | `tenants/{tenantId}/login-history` | Historique connexions d'un tenant |

### Tenants — `/api/v{version}/admin/tenants`

| Méthode | Route | Description |
|---|---|---|
| POST | `/` | Créer tenant |
| GET | `/` | Liste tenants |
| GET | `{id}` | Tenant par ID |
| GET | `by-slug/{slug}` | Tenant par slug |
| PUT | `{id}` | Modifier tenant |
| DELETE | `{id}` | Supprimer tenant |

### Users/Tenant — `/api/v{version}/{tenantId}/users`

| Méthode | Route | Description |
|---|---|---|
| POST | `/` | Créer utilisateur |
| GET | `/` | Liste utilisateurs |
| GET | `{id}` | Utilisateur par ID |
| PUT | `{id}` | Modifier utilisateur |
| DELETE | `{id}` | Supprimer utilisateur |
| POST | `{id}/change-password` | Changer mot de passe |
| POST | `forgot-password` | Envoyer réinitialisation |

### Roles — `/api/v{version}/{tenantId}/roles`

| Méthode | Route | Description |
|---|---|---|
| POST | `/` | Créer rôle |
| GET | `/` | Liste rôles |
| PUT | `{id}` | Modifier rôle |
| DELETE | `{id}` | Supprimer rôle |
| POST | `{roleId}/users/{userId}` | Assigner rôle |
| DELETE | `{roleId}/users/{userId}` | Retirer rôle |
| GET | `user/{userId}` | Rôles d'un utilisateur |

### Permissions — `/api/v{version}/{tenantId}/permissions`

| Méthode | Route | Description |
|---|---|---|
| POST | `/` | Créer permission |
| GET | `/` | Liste permissions |
| GET | `grouped` | Permissions groupées |
| DELETE | `{id}` | Supprimer permission |
| POST | `assign` | Assigner permissions à un rôle |

### Clients OAuth2 — `/api/v{version}/admin/tenants/{tenantId}/clients`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Liste clients |
| GET | `{clientId}` | Client par ID |
| POST | `/` | Créer client |
| PUT | `{clientId}` | Modifier client |
| DELETE | `{clientId}` | Supprimer client |

### API Keys — `/api/v{version}/admin/tenants/{tenantId}/api-keys`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Liste clés API |
| GET | `{keyId}` | Clé par ID |
| POST | `/` | Créer clé API |
| POST | `{keyId}/rotate` | Rotation clé |
| DELETE | `{keyId}` | Supprimer clé |

### External Services — `/api/v{version}/external-services`

| Méthode | Route | Description |
|---|---|---|
| POST | `register` | Enregistrer service externe |
| GET | `/` | Mes services |
| GET | `{id}` | Service par ID |
| PATCH | `{id}` | Modifier service |
| DELETE | `{id}` | Supprimer service |
| GET | `by-slug/{slug}` | Résoudre slug → URL (public) |
| GET | `_internal/active` | Services actifs (interne) |
| POST | `_internal/{id}/health-result` | Résultat health check (interne) |

### GDPR — `/api/v{version}/account`

| Méthode | Route | Description |
|---|---|---|
| POST | `consent` | Enregistrer consentement |
| GET | `consent` | Historique consentements |
| GET | `data` | Exporter données (ZIP) |
| POST | `forget-me` | Anonymiser compte |

### Support — `/api/v{version}/support`

| Méthode | Route | Description |
|---|---|---|
| POST | `report` | Signaler bug / ticket support |

### Diagnostics — `/api/v{version}/diagnostics`

| Méthode | Route | Description |
|---|---|---|
| GET | `health` | Health check |

### OIDC

| Méthode | Route | Description |
|---|---|---|
| GET/POST | `/connect/authorize` | Authorize endpoint |
| GET/POST | `/connect/logout` | Logout endpoint |

---

## DotnetNiger.Identity.Web — Developer Portal

Portail développeur (Razor Pages) authentifié via OpenID Connect.

| Accès | Route | Description |
|---|---|---|
| Public | `/` | Accueil |
| Public | `/Account/Login` | Connexion OIDC |
| Auth | `/Account/Logout` | Déconnexion |
| Public | `/Account/AccessDenied` | Accès refusé |
| Public | `/Docs` | Documentation |
| Public | `/Status` | Statut des services |
| Public | `/Support` | Support |
| Public | `/Securite` | Sécurité |
| Public | `/Confidentialite` | Confidentialité |
| Public | `/ConditionsUtilisation` | Conditions d'utilisation |
| Auth | `/Developer` | Hub développeur |
| Auth | `/Developer/Dashboard` | Tableau de bord (statistiques) |
| Auth | `/Developer/Profile` | Gestion profil (2FA, email, mot de passe, historique) |
| Auth | `/Developer/ApiKeys` | Gestion clés API |
| Auth | `/Developer/Services` | Gestion services externes |
| Auth | `/Developer/Gdpr` | Zone GDPR (consentement, export, anonymisation) |
| Auth | `/Developer/Securite` | Page sécurité (sessions, 2FA) |
| Auth | `/Developer/Docs` | Documentation intégration |
| Admin | `/Developer/Admin` | Dashboard admin |
| Admin | `/Developer/Admin/Invite` | Inviter admin |
| Admin | `/Developer/Admin/AuditLogs` | Journal d'audit |
| Admin | `/Developer/Admin/Tenants` | Gestion tenants (CRUD) |
| Admin | `/Developer/Admin/Tenants/Clients` | Clients OAuth2 par tenant |
| Admin | `/Developer/Admin/Tenants/Roles` | Rôles par tenant |
| Admin | `/Developer/Admin/Tenants/ApiKeys` | Clés API par tenant |
| Admin | `/Developer/Admin/Tenants/Permissions` | Permissions par tenant |
| Admin | `/Developer/Admin/Tenants/TenantUsers` | Utilisateurs par tenant |
| Admin | `/Developer/Admin/Tenants/LoginHistory` | Historique connexions par tenant |

---

## DotnetNiger.Community — API Endpoints

Base: `https://community-dotnetniger.runasp.net/api/v{version}` (via Gateway: `https://dotnetniger.runasp.net/api/{...}`)

### Posts — `/api/v{version}/posts`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister articles (filtres: published, category, tag, query, page, pageSize) |
| GET | `{id}` | Article par ID |
| GET | `{slug}` | Article par slug (regex) |
| GET | `by-slug/{slug}` | OG meta pour article |
| GET | `mine` | Mes articles (authentifié) |
| POST | `/` | Créer article |
| PUT | `{id}` | Modifier article |
| PATCH | `{id}/publish` | Publier article |
| PATCH | `{id}/unpublish` | Dépublier article |
| POST | `{id}/views` | Incrémenter vues |
| DELETE | `{id}` | Supprimer article |

### Events — `/api/v{version}/events`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister événements (filtres: published, past, eventType, query, tag, dates) |
| GET | `mine` | Mes événements (authentifié) |
| GET | `upcoming` | Événements à venir |
| GET | `{id}` | Événement par ID |
| GET | `{slug}` | Événement par slug |
| GET | `by-slug/{slug}` | OG meta pour événement |
| POST | `/` | Créer événement |
| PUT | `{id}` | Modifier événement |
| DELETE | `{id}` | Supprimer événement |
| POST | `registrations` | S'inscrire à un événement |
| DELETE | `{eventId}/registrations` | Annuler inscription |
| GET | `{eventId}/registrations` | Inscriptions à un événement |
| GET | `pending` | Événements en attente (Admin) |
| PATCH | `{id}/approve` | Approuver événement (Admin) |
| PATCH | `{id}/reject` | Rejeter événement (Admin) |

### Comments — `/api/v{version}/comments`

| Méthode | Route | Description |
|---|---|---|
| GET | `post/{postId}` | Commentaires d'un article |
| GET | `event/{eventId}` | Commentaires d'un événement |
| GET | `{id}` | Commentaire par ID |
| POST | `/` | Créer commentaire |
| PUT | `{id}` | Modifier commentaire |
| DELETE | `{id}` | Supprimer commentaire |

### Profile — `/api/v{version}/profile`

| Méthode | Route | Description |
|---|---|---|
| GET | `me` | Mon profil |
| PUT | `me` | Modifier mon profil |
| GET | `social-links` | Mes liens sociaux |
| POST | `social-links` | Ajouter lien social |
| DELETE | `social-links/{id}` | Supprimer lien social |
| POST | `certificates` | Soumettre certificat |

### Resources — `/api/v{version}/resources`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister ressources (filtres: resourceType, level, query, tag, categoryId, createdBy) |
| GET | `mine` | Mes ressources (authentifié) |
| GET | `{id}` | Ressource par ID |
| GET | `{slug}` | Ressource par slug |
| GET | `by-slug/{slug}` | OG meta pour ressource |
| GET | `types` | Types de ressources distincts |
| GET | `levels` | Niveaux distincts |
| POST | `/` | Créer ressource |
| PUT | `{id}` | Modifier ressource |
| DELETE | `{id}` | Supprimer ressource |
| POST | `{id}/views` | Incrémenter vues |

### Projects — `/api/v{version}/projects`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister projets (filtres: status, query) |
| GET | `featured` | Projets à la une |
| GET | `{id}` | Projet par ID |
| GET | `slug/{slug}` | Projet par slug |
| POST | `/` | Créer projet |
| PUT | `{id}` | Modifier projet |
| DELETE | `{id}` | Supprimer projet |

### Categories — `/api/v{version}/categories`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister catégories |
| GET | `{id}` | Catégorie par ID |
| GET | `{slug}` | Catégorie par slug |
| POST | `/` | Créer catégorie (Admin) |
| PUT | `{id}` | Modifier catégorie (Admin) |
| DELETE | `{id}` | Supprimer catégorie (Admin) |

### Tags — `/api/v{version}/tags`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister tags |
| GET | `{id}` | Tag par ID |
| GET | `{slug}` | Tag par slug |
| POST | `/` | Créer tag (Admin) |
| PUT | `{id}` | Modifier tag (Admin) |
| DELETE | `{id}` | Supprimer tag (Admin) |

### Members — `/api/v{version}/members`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Lister membres (filtres: query, country) |
| GET | `{id}` | Membre par ID |

### Partners — `/api/v{version}/partners`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Partenaires actifs (filtre: partnerType) |
| GET | `{id}` | Partenaire par ID |
| POST | `/` | Créer partenaire (Admin) |
| PUT | `{id}` | Modifier partenaire (Admin) |
| DELETE | `{id}` | Supprimer partenaire (Admin) |

### Admin — `/api/v{version}/admin`

| Méthode | Route | Description |
|---|---|---|
| GET | `dashboard` | Stats tableau de bord (Admin) |
| GET | `events` | Tous les événements (Admin) |
| PATCH | `events/{id}/publish` | Publier événement (Admin) |
| PATCH | `events/{id}/unpublish` | Dépublier événement (Admin) |
| PATCH | `events/{id}/approve` | Approuver événement (Admin) |
| PATCH | `events/{id}/reject` | Rejeter événement (Admin) |
| GET | `users` | Tous les utilisateurs (Admin) |
| GET | `users/{id}` | Utilisateur par ID (Admin) |
| PATCH | `users/{id}/status` | Statut utilisateur (Admin) |
| PATCH | `users/{id}/team` | Équipe utilisateur (Admin) |
| POST | `users` | Créer utilisateur (Admin) |
| DELETE | `users/{id}` | Supprimer utilisateur (Admin) |
| GET | `roles` | Lister rôles (Admin) |
| POST | `roles` | Créer rôle (Admin) |
| GET | `permissions` | Lister permissions (Admin) |
| POST | `permissions` | Créer permission (Admin) |
| POST | `roles/{roleId}/permissions` | Assigner permission à rôle (Admin) |
| POST | `users/{userId}/roles` | Assigner rôle à utilisateur (Admin) |

### Upload — `/api/v{version}/upload`

| Méthode | Route | Description |
|---|---|---|
| POST | `/` | Upload image (fichier, max 2MB) |
| POST | `base64` | Upload image (base64) |
| DELETE | `/` | Supprimer fichier uploadé |

### Newsletter — `/api/v{version}/newsletter`

| Méthode | Route | Description |
|---|---|---|
| POST | `subscribe` | S'abonner |
| POST | `unsubscribe` | Se désabonner |
| GET | `/` | Abonnements (Admin) |
| GET | `count` | Nombre d'abonnés actifs |
| DELETE | `{email}` | Supprimer un abonné par email (Admin) |

### Contact — `/api/v{version}/contact`

| Méthode | Route | Description |
|---|---|---|
| POST | `/` | Envoyer message contact |

### Notifications — `/api/v{version}/notifications`

| Méthode | Route | Description |
|---|---|---|
| GET | `{userId}` | Notifications d'un utilisateur |
| GET | `{userId}/unread-count` | Nombre de notifications non lues |
| POST | `{userId}` | Envoyer notification |
| PATCH | `{userId}/{id}/read` | Marquer comme lue |
| PATCH | `{userId}/read-all` | Tout marquer comme lu |

### Stats — `/api/v{version}/stats`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Statistiques plateforme |

### Search — `/api/v{version}/search`

| Méthode | Route | Description |
|---|---|---|
| GET | `/` | Recherche multi-contenus (posts, events, resources, projects) |

### Diagnostics — `/api/v{version}/test`

| Méthode | Route | Description |
|---|---|---|
| GET | `health` | Health check |

---

## Gateway — Ocelot Routes

La Gateway écoute sur `https://dotnetniger.runasp.net` et proxyfie vers Identity et Community.

### Routes vers Community

| Méthode(s) | Chemin Amont | Cache | Rate Limit |
|---|---|---|---|
| GET | `/api/posts/{everything}` | 10s | Oui |
| POST/PUT/DELETE/PATCH | `/api/posts/{everything}` | — | 50/m |
| GET | `/api/comments/{everything}` | — | — |
| GET | `/api/events/{everything}` | 15s | — |
| GET | `/api/resources/{everything}` | 20s | — |
| GET | `/api/categories/{everything}` | 30s | — |
| GET | `/api/tags/{everything}` | 30s | — |
| GET | `/api/stats/{everything}` | 15s | — |
| GET | `/api/search/{everything}` | 30s | 30/m |
| POST | `/api/newsletters/subscribe` | — | 10/m |
| POST | `/api/newsletters/unsubscribe` | — | 10/m |
| POST | `/api/upload` | — | 10/m (QoS 30s) |
| GET/POST/PUT/DELETE/PATCH | `/api/community/admin/{everything}` | — | 30/m |
| Tous | `/api/projects/{everything}`, `/api/partners/{everything}`, `/api/members/{everything}`, `/api/profile/{everything}`, `/api/contact`, `/api/notifications/{everything}` | — | — |

### Routes vers Identity

| Méthode(s) | Chemin Amont | Rate Limit | QoS |
|---|---|---|---|
| GET/POST | `/api/auth/{everything}` | 30/m | Oui |
| POST | `/api/auth/forgot-password` | 5/m | Oui |
| POST | `/api/auth/reset-password` | 5/m | Oui |
| POST | `/connect/token` | 30/m | Oui |
| POST | `/api/auth/refresh` | — | Oui |
| POST | `/api/auth/request-email-verification` | 5/m | Oui |
| POST | `/api/auth/verify-email` | 5/m | Oui |
| GET/POST | `/connect/authorize` | — | — |
| GET/POST | `/connect/logout` | — | — |
| GET/POST | `/connect/userinfo` | — | — |
| GET | `/.well-known/{everything}` | — | — |
| GET/POST | `/api/super-admin` | 30/m | Oui |
| GET/PUT | `/api/profile/{everything}` | — | — |
| Tous | `/api/tenants/{everything}`, `/api/external-services/{everything}`, `/api/identity/admin/{everything}`, `/api/diagnostics/{everything}` | — | — |

---

## Développement local

### Prérequis

- .NET 9 SDK
- SQL Server (LocalDB, Docker, ou instance distante)
- Visual Studio 2022 / Rider / VS Code

### Configurer les bases de données

Chaque projet a son propre `appsettings.Development.json` :

- `DotnetNiger.Identity` → base Identity
- `DotnetNiger.Community` → base Community
- `DotnetNiger.Gateway` → pas de base directe
- `DotnetNiger.Identity.Web` → pas de base directe

Les fichiers `appsettings.Development.json` sont gitignorés. Copier depuis `appsettings.json` et adapter les chaînes de connexion.

### Lancer les projets

```bash
# Démarrer Identity (port 5075)
cd DotnetNiger.Identity
dotnet run

# Démarrer Community (port 5050)
cd DotnetNiger.Community
dotnet run

# Démarrer Gateway (port 5000)
cd DotnetNiger.Gateway
dotnet run

# Démarrer Identity.Web (port 5100)
cd DotnetNiger.Identity.Web
dotnet run
```

### Docker (alternative)

```bash
docker compose up -d
```

Lance SQL Server + les 4 services avec une config dev automatique.

---

## Déploiement production

Le déploiement utilise le workflow GitHub Actions **deploy.yml** sur la branche `BackEnd` :

1. **CI** (`.github/workflows/ci.yml`) : Restore + Build Release, warnings bloquants
2. **Deploy** (`.github/workflows/deploy.yml`) : `dotnet publish` chaque projet → push vers les branches `deploy/identity`, `deploy/identity-web`, `deploy/community`, `deploy/gateway`

Chaque branche `deploy/*` contient les binaires pré-compilés. Le serveur (Hostinger) est configuré pour déployer automatiquement depuis ces branches.

### wwwroot/uploads

Les fichiers uploadés par les utilisateurs (`wwwroot/uploads/`) sont exclus du publish et stockés uniquement sur le serveur. Ne pas les supprimer lors des déploiements.

### Configuration production

Les `appsettings.Production.json` sont gitignorés. Configurer le serveur avec :
- Variables d'environnement pour les secrets (JWT, SMTP, OAuth, connexions DB)
- Ou déposer un `appsettings.Production.json` sur le serveur après le premier déploiement

---

## Licence

Propriété de DotnetNiger Community. Tous droits réservés.
