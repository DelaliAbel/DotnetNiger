# Changelog

Le format est base sur [Keep a Changelog](https://keepachangelog.com/fr/1.0.0/) et ce projet adhere au [Semantic Versioning](https://semver.org/lang/fr/).

## [Unreleased]

### Added
- Tag/Category sync dans EventCommandService, PostCommandService, ResourceCommandService
- Endpoint `GET /api/admin/stats/mine` pour le dashboard Collaborateur
- CRUD Membres (POST/PUT/DELETE `/api/members`), endpoint `GET /api/members/mine`
- Email notification admin pour les messages de contact
- `ContactMessagesController` (GET list, PATCH mark as read)
- `PublicSettingsController` à `GET /api/settings/public`

### Changed
- Program.cs : CORS depuis `appsettings.json`, suppression middleware debug, refactoring 319→45 lignes
- Architecture Onion : API restructurée en Domain/Application/Infrastructure/Api
- UI restructurée : Services découpés en Api/Mock/App/Auth/Contracts/Browser
- Format réponse unifié `{ success, data, message }` sur les 22 controllers
- `BaseController` : helpers `Success<T>()`, `Failure()`, `NotFound()` etc.
- `ContactMessage.Name` remplacé par `FullName`

### Fixed
- Tags/Categories effaces a l'update : backend ne remplace plus si `null`
- CORS : plus de `AllowAnyOrigin()` en dur
- Tous les Update DTOs frontend (Event, Resource, Post, Project) rendus nullable pour corriger les PUT 400
- Routes API frontend synchronisees avec le backend (auth, admin, profile)
- `Sidebar.razor` : NewsLetter et Commentaires caches pour les Collaborateurs
- `RedirectToLogin.razor` : utilisateur connecte sans role redirige vers `/admin` au lieu de `/`
- `SocialLinkConfiguration.WithMany(m => m.SocialLinks)` corrigé
- Settings.razor : tabs inutilisés et variables supprimés

### Removed
- `SocialLinkRequest.cs` (inutilisé)
- Méthodes `AddSocialLinkAsync`/`RemoveSocialLinkAsync` de `IMemberDirectoryService`
- Clés config inutilisées : `Admin:DefaultPassword`, `Serilog`, `DatabaseProvider`, `Smtp:AppBaseUrl`

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
