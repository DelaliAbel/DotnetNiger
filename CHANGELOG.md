# Changelog

Toutes les modifications notables de ce projet sont documentées dans ce fichier.

Le format est basé sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/),
et ce projet adhère au [Semantic Versioning](https://semver.org/lang/fr/).

---

## [Unreleased]

### Added
- **Tag/Category sync** dans `EventCommandService`, `PostCommandService`, `ResourceCommandService` : synchronisation complète des tags (`TagNames`, `TagIds`) et catégories (`CategoryIds`) à la création et à la mise à jour
- **Endpoint `GET /api/v1/admin/stats/mine`** : statistiques personnelles pour le dashboard Collaborateur (events, blogs, resources, projects count)
- **CRUD Membres** : `POST /api/v1/members` (créer profil), `PUT /api/v1/members/{id}` (modifier), `DELETE /api/v1/members/{id}` (supprimer)
- **Endpoint `GET /api/v1/members/mine`** (via `GetProfileAsync`) : récupération du profil du user connecté
- **CI/CD corrigé** : workflows `.github/workflows/ci.yml` et `deploy.yml` utilisent `DotnetNiger.sln` et publient vers `deploy/backend` / `deploy/frontend`
- **Docker** : `docker-compose.yml` simplifié (SQL Server uniquement), suppression des services orphelins
- **Fichier `FRONTEND_TASKS.md`** : liste des tâches UI/UX pour le développeur frontend

### Changed
- **Program.cs** : suppression du middleware de debug claims-logging ; CORS utilise maintenant `Cors:AllowedOrigins` depuis `appsettings.json`
- **ResourceResponse** : ajout du champ `Url` (manquant dans le DTO)
- **MembersController** : hérite de `BaseController` pour accéder à `GetUserId()`, `IsAdmin()`, etc.
- **README.md** : mise à jour complète pour refléter l'architecture monolithique (4 projets : Domain, Infrastructure, Server, Client)

### Fixed
- **Tags/Catégories effacés à l'update** : le backend ne remplace plus les associations si `TagNames`/`CategoryIds` est `null` (seulement si fourni, même liste vide)
- **CORS** : plus de `AllowAnyOrigin()` en dur, lecture depuis la config
- **CI/CD** : correction de la référence `.slnx` → `.sln` ; suppression des 4 projets inexistants (Identity, Identity.Web, Community, Gateway)

### Removed
- Middleware debug claims dans `Program.cs` (lignes 128-139)
- Services Docker orphelins dans `docker-compose.yml`
- Anciens workflows de deploy vers branches `deploy/identity*`, `deploy/community`, `deploy/gateway`

---

## [2026-07-19] — Consolidation monolithique & Fixes critiques

### Added
- Fusion des 4 anciens repos (Identity, Community, Gateway, Identity.Web) en **un seul repo monolithique** avec 4 projets .NET
- Single-role enforcement : un user = un seul rôle (le nouveau remplace l'ancien)
- `TokenPrincipalBuilder` : double claim `ClaimTypes.Role` + `"role"` pour validation OpenIddict locale
- Admin endpoints : `GET /api/v1/admin/users`, `GET /api/v1/admin/stats`, `POST /api/v1/admin/invite`
- Update endpoints corrigés (Events, Posts, Resources, Projects) : ownership checks, champs nullable, retour 404/403 appropriés
- Frontend single-role UI : `AdminActionDropdown.razor` et `ViewUser.razor` passent de toggles à radio-select
- Refactoring >200 lignes : 15 fichiers splittés en `partial class` (tous < 250 lignes)

### Changed
- Target framework : Client `net8.0` / Backend `net9.0`
- Architecture : monolithique modulaire (Domain, Infrastructure, Server, Client)

### Fixed
- 403 `insufficient_access` sur endpoints admin (double claim role)
- Ownership checks manquants sur PUT/DELETE
- Champs non mappés dans Update DTOs (EventType, Category, IsPublished, Url, etc.)

### Removed
- Anciens projets séparés (Identity, Community, Gateway, Identity.Web)
- Dockerfiles pour les 4 anciens projets
- `docker-compose.prod.yml`
- Projets de tests et architecture guards

---

## [2026-06-25]

### Fixed
- Gateway CORS preflight et réponses 500
- Configuration HTTPS downstream
- JWT metadata HTTPS requirement désactivé pour gateway

---

## [2026-06-24]

### Added
- Scripts de création SQL Server pour Identity et Community

### Fixed
- Suppression auto-migration/seed au profit des scripts SQL
- Routage Gateway pour `/Account/*` vers Portal

---

## [2026-06-23]

### Added
- Workflow Deploy (push vers branches `deploy/*`)
- Configuration déploiement MonsterASP
- Infrastructure seeding DB

---

## [2026-06-22]

### Added
- Gestion Team Members (endpoints)
- Authentification par API Key Identity pour endpoints internes
- Admin CRUD endpoints
- Rôles SuperAdmin (Identity) et Collaborator (Community)
- Système Open Graph social preview

### Fixed
- Problèmes de sécurité pour déploiement MonsterASP

---

## [2026-06-20]

### Added
- Entité `MemberSkill` et support Skills pour profils membres
- Routes Gateway pour upload base64 et fichiers statiques

---

## [2026-06-19]

### Fixed
- Gestion `DbUpdateException` concurrente dans ProfileService