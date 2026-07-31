# Changelog

## 2026-07-31
- **feat** : dynamisation des paramètres site avec upload logo et harmonisation réseaux sociaux (PublicSettingsResponse, Settings admin, Footer/TopBar/Home/Contact)
- **fix** : callback OAuth `external-login` construit depuis l'API (`Request.Scheme://{Request.Host}/api/auth/external-callback`) au lieu de `FrontendBaseUrl` (404 sur le frontend)
- **fix** : harmonisation `CancellationToken` sur `ISettingsService` (GetAllAsync/GetByKeyAsync/SetAsync/SetBatchAsync/DeleteAsync)
- **fix** : reset password — champ token masqué (lu depuis l'URL), toggle visibilité mot de passe, payload `newPassword` (binding API) + décodage URL token/email, validation frontend alignée sur les règles Identity
- **config** : `Uploads__Path` configurable (`UploadOptions`) — uploads vers `private/uploads` en prod
- **config** : connexion base prod `db61810` + revert UI appsettings (config non versionnée)
- **feat** : enrichissement certificats avec infos utilisateur (userId, userName, userEmail, avatarUrl, certificateUrl, certificateType, reviewedNotes, reviewedAt) + chargement Member/User via Include avec fallback
- **mise à jour** : retouches UI admin (Dashboard, MyBlog/MyEvents/MyProjects, Users — harmonisation ombres/fonds)
- **config** : CORS ajout `https://localhost:7104`
- **chore** : merge origin/dev (paramètres site dynamiques, certificats)

## 2026-07-30
- **fix** : suppression des inscriptions et commentaires avant de supprimer un événement (FK restrict)
- **fix** : CancellationToken ajouté à toutes les méthodes `async Task` dans Services/ (82 fichiers)
- **fix** : packages Asp.Versioning.Mvc/Mvc.ApiExplorer supprimés du `.csproj` (dead code)
- **fix** : migration `RemoveRedundantBooleanFields` (Post.IsPublished, Event.IsPublished/IsArchived, Comment.AuthorId)
- **fix** : mot de passe admin déplacé de la constante hardcodée vers `appsettings.json:AdminPassword`
- **fix** : PermissionNames alias CRUD pointent vers `content.events.moderate` (au lieu de `admin.dashboard.view`)
- **fix** : middleware gère `UnauthorizedAccessException`→403, `KeyNotFoundException`→404, `InvalidOperationException`→400 (JSON d'erreur standardisé)
- **fix** : `ApiCommentService.CreateCommentAsync` et `ApiProjectService.CreateAsync` lancent `InvalidOperationException` (pattern cohérent avec Post/Event)
- **fix** : `LogoutAsync` appelle `ClearTokensAsync(clearAllStorage: false)` (préserve localStorage non-auth)
- **fix** : `ConfirmService.ShowAsync` fusionné (CancellationToken + IJSRuntime, merge conflit origin/dev)
- **mise à jour** : thème (pages auth, community)
- **mise à jour** : ConfirmModal component + container
- **mise à jour** : AdminActionDropdown, Editor, ImageUploader, TinyMCE init
- **clean** : suppression `.dockerignore`, mise à jour `.md` (README, CHANGELOG, CONTRIBUTING, SECURITY)
- **clean** : suppression workflows deploy (frontend GitHub Pages, backend MonsterASP)
- **docs** : README rôles membres mis à jour, section déploiement supprimée

## 2026-07-29
- **fix** : envoie `UpdateEventRequest` au PUT API pour corriger la mise à jour des événements
- **fix** : `SyncEventSpeakers` contourne la navigation collection EF Core (UPDATE vs INSERT)
- **fix** : `SyncEventSpeakers` utilise `RemoveRange` pour éviter `DbUpdateConcurrencyException`
- **mise à jour** : pagination blog, ressource, event
- **mise à jour** : thème page contact
- **mise à jour** : page partenaire
- **mise à jour** : page community
- **mise à jour** : page home

## 2026-07-28
- **refactor** : Architecture Onion (Domain → Application → Infrastructure → Api) + UI restructure + correctifs P1-P4
- **clean** : suppression clés config inutilisées (`Admin:DefaultPassword`, `FrontendBaseUrl`, `Serilog`, `DatabaseProvider`, `Smtp:AppBaseUrl`)
- **clean** : suppression dotfiles obsolètes (`.env.example`, `.gitattributes` UI, `.gitignore` UI)
- **clean** : suppression workflow Docker
- **docs** : mise à jour README, CHANGELOG, SECURITY
- **bugfix** : vue détail utilisateur depuis l'admin

## 2026-07-27
- **fix** : audit complet — soft-delete, race conditions, NRE, auth, email links, UI `OnParametersSetAsync`

## 2026-07-26
- **fix** : correction complète register, rate limiting, events/posts, auth, permissions, security

## 2026-07-25
- **mise à jour** : ajustement du thème

## 2026-07-24
- **feat** : amélioration admin, contenus (events/resources), profil utilisateur et UI
- **feat** : ajout endpoint `/health` pour monitoring
- **refactor** : pages admin, services, nettoyage docs
- **fix** : accès dashboard collaborateur, update profil, team display, upload routing
- **fix** : isolation contenu collaborateur, URL auth guard
- **fix** : `.gitignore` uploads chemin avec `**`
- **fix** : collaborateur projets, Comments/Newsletter admin-only
- **fix** : visibilité événements en validation + `CreatedBy` collaborateur
- **fix** : redirection admin par rôle, upload blog, social-links, events status
- **fix** : résolution URLs upload, réseaux sociaux, toast
- **fix** : suppression `messageSocial`, toast succès/erreur
- **mise à jour** : thèmes

## 2026-07-23
- **fix** : correction bug login/register
- **refactor** : réorganisation dossiers API (suppression dossiers vides/doublons)

## 2026-07-22
- **feat** : merge frontend develop dans `DotnetNiger.UI` + correction wiring backend
- **fix** : 12 correctifs sécurité/flux auth

## 2026-07-21
- **fix** : workflows CI sur toutes les branches, déploiement sur master uniquement
- **fix** : warnings CS860x dans BlogEdit.razor (0 warning 0 error)
- **fix** : redirection login vers `/login` (Blazor) au lieu de `/auth/login` et `/Account/Login`
- **fix** : TokenService capture `UnauthorizedAccessException`
- **fix** : décompression Brotli/GZip/Deflate du token endpoint
- **docs** : mise à jour CHANGELOG, README, TODO

## 2026-07-20
- **fix** : suppression versionning API, correction routes, Swagger, accès public events, migration DB auto
- **feat** : bouton Authorize Bearer JWT dans Swagger
- **fix** : redirection login Blazor, Razor Pages pour `/Account/Login`
- **fix** : warnings CS1998/CS8601
- **fix** : config OpenIddict (HTTPS dev, external_login, scopes, ClientSetupService)
- **docs** : nettoyage .md minimaliste

## 2026-07-19
- **merge** : fusion multi-repo → monolithe (Common, Community, Gateway, Identity, Identity.Web, Tests, docs supprimés)
- **ci** : workflow build multi-projet (net9.0 backend + net8.0 frontend)
- **clean** : suppression anciens dossiers de l'architecture multi-repo
- **mise à jour** : appsettings

## 2026-07-18
- **fix** : correction bugs critiques updates (Event, Blog, Resource)

## 2026-07-17
- **feat** : ajout provider OAuth Microsoft
- **feat** : endpoint public settings pour OG middleware
- **feat** : image OG par défaut, route Ocelot /images/
- **feat** : alignement CORS Identity/Community avec Gateway
- **feat** : route Ocelot /signin-microsoft
- **feat** : log code confirmation en mode dev
- **docs** : guide OAuth providers
- **chore** : arrêt tracking OAUTH_PROVIDERS_SETUP.md (credentials)

## 2026-07-14
- **fix** : multi-roles JWT, cache invalidation, Swagger JWT, descriptions XML
- **feat** : public team endpoint + social preview middleware

## 2026-07-11
- **fix** : slug (fallback unicode, NFKD/NFKC)
- **fix** : suppression rôle utilisateur, skills, team management
- **fix** : profile UpdateSkills compatibilité EF Core
- **fix** : redirect external-callback-frontend vers FrontendBaseUrl

## 2026-07-10
- **fix** : uniformisation appsettings, nettoyage secrets
- **fix** : finalisation appsettings prod
- **fix** : CORS Gateway (URLs Identity/Community)
- **fix** : gestion erreurs Login/Register/ForgotPassword, uniformisation .md
- **fix** : redirect login ReturnUrl
- **feat** : uniformisation design auth pages + emails + page ConfirmEmail
- **clean** : suppression package.json racine

## 2026-07-09
- **feat** : config email SMTP Gmail pour confirmation

## 2026-07-08
- **feat** : endpoints `/mine`, suppression admin newsletter, settings AdminOrSuperAdmin
- **fix** : track `wwwroot/uploads/.gitkeep` pour GitHub Actions
- **chore** : gitignore DbManager

## 2026-07-05
- **refactor** : mise en commun dans `DotnetNiger.Common`, retrait logique des contrôleurs

## 2026-07-02
- **feat** : permissions, certificats, team member et améliorations diverses

## 2026-06-30
- **fix** : HTTPS scheme Identity OAuth redirect_uri
- **fix** : DelegatingHandler forward X-Forwarded-* via Ocelot
- **fix** : SameSite=None Secure auth cookies
- **fix** : duplicate WebApplication.CreateBuilder (crash IIS)
- **fix** : soumission certificat anonyme (fallback UserId)
- **feat** : ImageUrlRewriteHandler + routes upload protégées

## 2026-06-29
- **fix** : CORS Identity/Community pour Blazor WASM
- **fix** : warnings CS8601 JWT config (TreatWarningsAsErrors)
- **fix** : align Gateway downstream HTTP
- **feat** : route OAuth callbacks via Gateway (HTTPS)
- **fix** : forward X-Forwarded-Proto Ocelot

## 2026-06-27
- **chore** : .gitignore complété pour tous les projets
- **refactor** : prepare backend for production deployment

## 2026-06-26
- **chore** : suppression projets test, migrations, docker, docs, assets inutilisés
- **feat** : BaseController helpers + Messages.cs centralisé (Community, Gateway)
- **refactor** : remplacement chaînes en dur par Messages.cs (contrôleurs, services, Gateway)
- **docs** : commentaires XML français (Community services, controllers, infrastructure, Gateway)
- **chore** : nettoyage (PR template, DTOs legacy, permissions/roles)

## 2026-06-25
- **fix** : auto-add `gt:external_login` permission web-ui client au démarrage
- **fix** : correction préfixe permission OpenIddict (`ep:` → `ept:`)
- **clean** : suppression fichier inutile

## 2026-06-24
- **fix** : Community/Identity remove auto-seed, scripts SQL Server
- **fix** : Gateway routes /Account/*, JWT button, JSON, HTTPS, CORS
- **fix** : ajout permission `gt:external_login` pour ticket-based login

## 2026-06-23
- **fix** : ForwardedHeaders, HTTPS config, dev SQLite
- **feat** : config URLs MonsterASP, DB seeding, workflow déploiement
- **fix** : NETSDK1152, 500.30 EnsureCreated SQL Server, permissions GitHub

## 2026-06-22
- **fix** : sécurité, préparation déploiement Monster ASP
- **feat** : restructuration rôles SuperAdmin + Collaborateur
- **feat** : team member management, api key auth, admin CRUD
- **fix** : suppression auto-seed, mise à jour downstream URLs

## 2026-06-21
- **feat** : Open Graph social preview (Gateway + Community)

## 2026-06-20
- **feat** : routes Gateway upload base64 + static files
- **feat** : MemberSkill entity + Skills profiles
- **fix** : bug AuthController

## 2026-06-19
- **fix** : Concurrent DbUpdateException ProfileService (SocialLink, Certificate, Update)

## 2026-06-18
- **fix** : routes v1 upstream community (me, social-links, contact, certificates)
- **fix** : NRE, race conditions, client-evaluation, mappings Community
- **fix** : try-catch email sending

## 2026-06-16
- **feat** : route Identity.Web via Gateway
- **feat** : déploiement Docker production + Hugging Face Spaces
- **feat** : enrich Community seeder (posts, events, resources, members, etc.)
- **fix** : health check messages, bypass Ocelot health, logs verbosity
- **docs** : mise à jour documentation Gateway

## 2026-06-14
- **feat** : upload controller, login ticket flow
- **fix** : post views endpoint, unique slug, Ocelot QoSOptions

## 2026-06-07
- **fix** : sécurité/ fiabilité Identity, Community (race condition, NRE, IDOR), Gateway
- **feat** : migration PendingEmail, pages admin Identity.Web
- **test** : 19 tests Identity, 12 tests Community, 13 tests Gateway
- **fix** : snapshot migration, requête LINQ non traduisible

## 2026-06-05
- **fix** : création dossier wwwroot Gateway (crash démarrage)

## 2026-06-02
- **feat** : routes Gateway /api/v1/ (projects, partners, members, newsletter)
- **feat** : endpoints slug, notifications, categories/tags/stats
- **perf** : AsNoTracking(), projections, keyset pagination, composite index
- **feat** : enrich ProfileResponse via JWT claims
- **fix** : route /me Identity → Community
- **fix** : code confirmation en mode dev
- **fix** : redirect ExternalCallbackFrontend, User-Agent GitHub API

## 2026-05-28
- **fix** : packages manquants (OpenIddict, IdentityModel, Sqlite)
- **fix** : crash endpoint search (EF Core Concat)

## 2026-05-27
- **feat** : Identity JWS tokens, GDPR, seed data enrichi
- **feat** : Community modules (member, newsletter, partner, project)
- **feat** : Gateway health check middleware
- **fix** : Release build errors/warnings (TreatWarningsAsErrors)
- **chore** : complétion TestIdentity + Identity.Web + docs

## 2026-05-26
- **fix** : Swagger toujours activé, OAuth email, password en dur, TestIdentity AccessDenied, catches vides

## 2026-05-25
- **fix** : OAuth/email config, admin UI, TestIdentity
- **fix** : CS1587, middleware order, shutdown crash, test failures
- **feat** : Gateway routes v1/ upstream, pass-through proxy
- **docs** : réécriture documentation Identity.Web/TestIdentity, merge CI, suppression Prettier

## 2026-05-14
- **feat** : Gateway dynamic service discovery + self-registration endpoint
- **fix** : CI .NET 9, packages vulnérables, warnings nullabilité
- **docs** : ajout guide déploiement (exclu git)

## 2026-05-11
- **refactor** : Community API alignée sur Identity
- **feat** : error handling middleware
- **refactor** : Gateway multi-service .NET 9 + clean architecture
- **merge** : Pull Request #12

## 2026-05-09
- **refactor** : Identity minimal OpenIddict multi-tenant
- **refactor** : Community API minimal
- **feat** : Dockerfile, health endpoint, docker-compose
- **docs** : integration guide, Identity README

## 2026-04-23
- **refactor** : code communauté
- **refactor** : optimisation conteneurs CI

## 2026-04-15
- **fix** : user management
- **chore** : package-lock bump

## 2026-03-23
- **feat** : dynamic feature toggle endpoints Community
- **refactor** : standardisation DTOs, pagination events/projects
- **fix** : Member/TeamMember DTO, accolades
- **docs** : documentation complète (CHANGELOG 1.0.0→1.4.0, ARCHITECTURE, API)

## 2026-03-14
- **refactor** : industrialisation couche API Community
- **refactor** : alignement conventions Identity/Community
- **refactor** : DTOs requests + services complémentaires
- **docs** : finalisation documentation
- **chore** : nettoyage artefacts

## 2026-03-11
- **feat** : remplacement domaine Team → Member
- **feat** : versioning API v1 + JWT
- **refactor** : services Community (IAdminService, IMemberService)
- **docs** : guide integration Blazor WASM, SETUP, INDEX, HEALTH_REPORT

## 2026-03-07
- **feat** : Ocelot routing natif (JWT, rate limiting, QoS, cache)
- **feat** : Admin endpoints Identity enrichis
- **feat** : base SQLite partagée Identity + Community
- **refactor** : suppression YARP Gateway
- **refactor** : nettoyage controllers Identity, scripts run.sh
- **docs** : HTTP collections, README, Dockerfile Gateway

## 2026-02-28
- **feat** : création Gateway Ocelot
- **feat** : ajout ocelot.json

## 2026-02-24
- **refactor** : Gateway (middlewares, exceptions)
- **feat** : Identity migrations + contrôleurs mis à jour
- **chore** : suppression run.ps1
- **format** : formatage style Gateway, doc, readme

## 2026-02-23
- **feat** : Community EF Core + SQLite DbContext, repositories, 10 contrôleurs
- **feat** : Seeder base complète (16 entités)
- **merge** : Pull Request #11

## 2026-02-21
- **feat** : Community EF Core + SQLite DbContext, repositories, controllers, migrations

## 2026-02-20
- **feat** : routes Gateway Community + Identity
- **feat** : CORS multi-program.cs
- **feat** : Identity (enums, interfaces, ApiKey HMAC-SHA256, validators, repository, DbFactory, JwtMiddleware)
- **feat** : Community (domaine, DTOs, contrôleurs)
- **feat** : Gateway (services, middlewares, pipeline)
- **docs** : documentation complète

## 2026-02-19
- **feat** : routes Gateway microservices
- **feat** : Team Coura Daga controller Community

## 2026-02-18
- **fix** : suppression client HTML/CSS/JS test Identity

## 2026-02-14
- **feat** : refactor services Identity (repository, cache, tokens)
- **fix** : ApikeyAuth, File Upload Service
- **docs** : ARCHITECTURE, identity endpoints
- **config** : .gitignore, Prettier workflow
- **test** : tests Identity

## 2026-02-11
- **fix** : mise en commentaire Azure Blob, ApiKeyService, AvatarCleanup

## 2026-02-10
- **feat** : password reset, email verification, change email
- **feat** : auth admin api-keys avatars
- **test** : unit + integration Identity
- **docs** : identity endpoints

## 2026-02-09
- **feat** : identity-auth, flux login/register/token
- **docs** : Architecture.md, DTOs
- **fix** : Prettier workflow, SQLite migration, Swagger config

## 2026-02-08
- **fix** : Prettier formatting
- **clean** : suppression weatherforecast.cs, team files

## 2026-02-07
- **fix** : architecture, suppression WeatherForecast
- **docs** : simplification documentation
- **fix** : run.sh et run.ps1

## 2026-02-04
- **feat** : ajout noms équipe dans Team file

## 2026-01-31
- **fix** : architecture, déplacement fichiers
- **clean** : nettoyage projets
- **fix** : controller Identity
- **test** : controller Community

## 2026-01-29
- **feat** : initial project setup, documentation, CI/CD
- **feat** : configuration Prettier workflow
- **fix** : workflow permissions, documentation
- **clean** : suppression deploy.yml, docker.yml
- **docs** : README, Initial plan
