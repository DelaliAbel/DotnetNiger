# Changelog

Tous les changements notables de DotnetNiger sont documentes dans ce fichier.

Le format suit Keep a Changelog et le versioning suit Semantic Versioning.

## [Unreleased]

### Added

- Gateway: `ServiceRegistry` singleton (`IServiceRegistry` / `ServiceRegistry`) pour la decouverte dynamique de services avec `ConcurrentDictionary` thread-safe.
- Gateway: `POST /api/service-registry/register` endpoint middleware pour l'enregistrement dynamique des services upstream (avec auth optionnelle via `X-Registration-Key`).
- Gateway: `Services/ServiceRegistry.cs` et `Services/ServiceRegistrationEndpoint.cs` — nouveaux fichiers pour le registre de services et le endpoint d'enregistrement.
- Identity: Auto-enregistrement aupres du Gateway au demarrage via `TryRegisterWithGatewayAsync()` (config `Gateway:RegistrationUrl` + `Gateway:RegistrationKey`).
- Community: Auto-enregistrement aupres du Gateway au demarrage via `TryRegisterWithGatewayAsync()` (config `Gateway:RegistrationUrl` + `Gateway:RegistrationKey`).

### Changed

- Gateway: `MapGatewayHealthEndpoints()` utilise desormais `IServiceRegistry.GetCombinedConfig()` au lieu d'une liste statique — les services dynamiques apparaissent dans `/health/downstream`, `/health/ready`, `/health/services`.
- Gateway: Swagger merge middleware utilise le registre de services (`IServiceRegistry`) pour decouvrir les endpoints Swagger des services upstream.
- Gateway: `Program.cs` cree un `ServiceRegistry` initialise depuis la config statique `DownstreamServices` et l'enregistre en singleton DI.
- Gateway: `DownstreamServiceConfig` reste utilise pour les routes Ocelot (construites au demarrage), le registre dynamique se superpose pour les health checks.
- docs/ARCHITECTURE.md: Mise a jour complete de la section "Dynamic Service Discovery" avec le nouveau modele a deux niveaux (statique + dynamique).
- docs/SETUP.md: Ajout des instructions de configuration du Gateway Registration, variables `Gateway:RegistrationUrl`/`Gateway:RegistrationKey`.
- DotnetNiger.Gateway/README.md: Ajout de la section Service Discovery, mise a jour du project structure, endpoints, et pipeline middleware.

### Fixed

- Community: Correction de la variable `combined` non declaree dans `SearchService.SearchAsync()` — ajout de `IQueryable<SearchResultResponse>? combined = null;`.

### Security

- Gateway: Le endpoint `/api/service-registry/register` peut etre protege par une cle API via `Gateway:RegistrationKey` (envoyee dans le header `X-Registration-Key`).

## [1.4.0] - 2026-03-14

### Added

- Community: industrialisation de la couche API et ajout de DTOs requests dedies.
- Identity: alignement des conventions de reponses et d'erreurs avec Community.

### Changed

- Projet: finalisation de la documentation transverse et mise a jour de la configuration.

### Removed

- Nettoyage d'artefacts obsoletes dans le depot.

## [1.3.0] - 2026-03-11

### Added

- Community: migration fonctionnelle Team -> Member.
- Community: versioning API v1 avec routes api/v{version}/... et configuration JWT.
- Documentation: guides d'integration Blazor WASM et mise a jour du setup/health/index.

### Changed

- Community: reorganisation des services et interfaces pour clarifier les responsabilites.

## [1.2.0] - 2026-03-07

### Added

- Gateway: routage Ocelot natif avec JWT, rate limiting, QoS et cache.
- Identity: enrichissement des endpoints admin et consolidation des routes de gestion utilisateurs.
- Infrastructure: configuration base SQLite partagee entre Identity et Community.

### Changed

- Documentation: re-ecriture de l'architecture et des guides pour refléter le gateway Ocelot natif.
- Scripts: consolidation de l'automatisation service/base dans run.sh.

### Removed

- Suppression de l'implementation gateway YARP depreciee.
- Nettoyage de controllers/dependances depreciees cote Identity.

## [1.1.0] - 2026-02-20

### Added

- Community: domaine complet (entites, enums, interfaces, DTOs) et controllers API.
- Identity: securite renforcee (HMAC API key, hash refresh tokens, middleware JWT, seeds roles/permissions/admin).
- Gateway: integration explicite des services et middlewares dans le pipeline.

### Changed

- Identity: refactor des services avec repository pattern et meilleure separation des responsabilites.
- Community: DI, repositories EF Core SQLite et seeding de donnees de test.

### Fixed

- Tests et configuration JWT: corrections de signatures et de cles minimales.
- Git/config: ajustements .gitignore et formatage pipeline.

## [1.0.0] - 2026-01-29

### Added

- Initialisation du projet et de l'architecture microservices.
- Premiers workflows CI/CD et outillage de formatage.
- Documentation de base du projet et du setup.

### Changed

- Iterations rapides sur la structure, README et workflows des les premiers jours.
