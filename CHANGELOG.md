# Changelog

Le format est base sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/) et ce projet adhere au [Semantic Versioning](https://semver.org/lang/fr/).

## [Unreleased]

### Added
- Tag/Category sync dans EventCommandService, PostCommandService, ResourceCommandService
- Endpoint `GET /api/admin/stats/mine` pour le dashboard Collaborateur
- CRUD Membres (POST/PUT/DELETE `/api/members`), endpoint `GET /api/members/mine`

### Changed
- Program.cs : CORS depuis `appsettings.json`, suppression middleware debug
- ResourceResponse : ajout du champ `Url`

### Fixed
- Tags/Categories effaces a l'update : backend ne remplace plus si `null`
- CORS : plus de `AllowAnyOrigin()` en dur
- Tous les Update DTOs frontend (Event, Resource, Post, Project) rendus nullable pour corriger les PUT 400
- Routes API frontend synchronisées avec le backend (auth, admin, profile)
- `Sidebar.razor` : NewsLetter et Commentaires cachés pour les Collaborateurs
- `RedirectToLogin.razor` : utilisateur connecté sans rôle redirigé vers `/admin` au lieu de `/`

### Added
- `BootstrapOpenIddictAsync` dans SeedData — enregistre le client OpenIddict "web-ui" au démarrage
- Redirect URI `/auth/callback-popup.html` ajouté au client web-ui (nécessaire pour le popup OAuth)
- `RoleConstants.IsSuperAdminRole()`

## [2026-07-19] — Consolidation monolithique

### Added
- Fusion de 4 anciens repos en un monolithe (4 projets .NET)
- Single-role enforcement
- Admin endpoints : users, stats, invite
- Update endpoints avec ownership checks

### Changed
- Target framework : Client net8.0 / Backend net9.0

### Fixed
- 403 `insufficient_access` sur endpoints admin
- Ownership checks manquants sur PUT/DELETE

### Removed
- Anciens projets (Identity, Community, Gateway, Identity.Web)
- Dockerfiles anciens projets
- Tests et architecture guards
