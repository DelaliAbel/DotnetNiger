# Changelog

## [Unreleased]

### Added
- Comprehensive README with full API documentation
- CI workflow for BackEnd branch

### Fixed
- OpenIddict endpoint permission prefix (`ep:` → `ept:`)
- Frontend comment crash (async CurrentUserId in WASM)
- Admin blog publish/unpublish toggle
- 503 on `/api/posts?published=true` (pageSize=6, exclude Content in listing)
- Certificate submission blocked by class-level `[Authorize]`
- CORS misconfiguration (unified to AllowAnyOrigin)

### Removed
- Test projects and architecture guards
- All sub-READMEs and integration guides
- Docs folder, Dockerfiles, favicons, .gitkeep files
- SQL creation scripts (superseded by EF Core seeding)

## [2026-06-25]

### Fixed
- Gateway CORS preflight and 500 error responses
- Downstream HTTPS configuration
- JWT metadata HTTPS requirement disabled for gateway

## [2026-06-24]

### Added
- SQL Server creation scripts for Identity and Community

### Fixed
- Remove auto-migration/seed in favor of SQL scripts
- Gateway routing for `/Account/*` to Portal

## [2026-06-23]

### Added
- Deploy workflow (push to deploy/* branches)
- MonsterASP deployment configuration
- DB seeding infrastructure

## [2026-06-22]

### Added
- Team member management endpoints
- Identity API key authentication for internal endpoints
- Admin CRUD endpoints
- SuperAdmin (Identity) and Collaborator (Community) roles
- Open Graph social preview system

### Fixed
- Security issues for MonsterASP deployment

## [2026-06-20]

### Added
- MemberSkill entity and Skills support for member profiles
- Gateway routes for upload base64 and static file serving

## [2026-06-19]

### Fixed
- Concurrent DbUpdateException handling in ProfileService
