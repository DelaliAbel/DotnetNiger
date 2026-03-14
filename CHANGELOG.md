# Changelog

Tous les changements notables dans DotnetNiger sont documentes dans ce fichier.

Le format est base sur Keep a Changelog et ce projet adhere a Semantic Versioning.

## [Unreleased]

### Added

- Community: DTOs request centralises pour controllers (Create/Update deplaces hors controllers)
- Community: endpoints PUT reactives pour events, projects et resources
- Community: client HTTP IIdentityApiClient implemente et endpoint GET /api/v1/test/identity-health
- Identity: endpoints diagnostics GET /api/v1/diagnostics/ping et GET /api/v1/diagnostics/health
- Gateway: priorites explicites sur routes admin (IdentityAdminRoute priorite 1, CommunityAdminRoute priorite 2)

### Changed

- Gateway: rate limiting explicite ajoute sur les routes non limitees pour eviter les blocages implicites
- Documentation: README et docs API alignes avec les routes Gateway actuelles

### Fixed

- Identity: warnings nullable CS8601 corriges dans AdminController
- Identity: warning restore NU1900 neutralise au niveau projet

## [1.0.0] - 2026-01-29

### Initial Release Items

- Initialisation de l'architecture microservices (Gateway, Identity, Community)
- API Gateway Ocelot avec Swagger agrege
- Service Identity (auth JWT, users, roles, permissions)
- Service Community (posts, comments, events, projects, resources)
- Documentation initiale et scripts de demarrage
