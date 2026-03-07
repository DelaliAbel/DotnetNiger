# Project Health Recap - DotnetNiger.Community

Date: 2026-03-07
Scope: scan des donnees statiques (hardcode) + sante globale du projet

## 1) Etat Global

- Build: OK (`dotnet build`) apres correction de `Application/Services/AdminService.cs`.
- Warning build: fichier executable verrouille temporairement (`MSB3026`, lock par `CompatTelRunner.exe`) mais compilation reussie.
- Couverture tests: tres faible.
- Fichiers C# vides: 62 fichiers detectes.

## 2) Donnees Statiques / Hardcode Detectees

### A. Endpoints avec reponses statiques directes

- `Api/Controllers/TeamController.cs`
  - Endpoint `GET /api/team/couranDaga`
  - Retourne une liste hardcodee de noms.
- `Api/Controllers/TestControllers.cs`
  - Endpoints de test/health avec payload hardcode.

### B. Seed de donnees (normal en dev, risqué si actif en prod)

- `Infrastructure/Data/Seeds/DatabaseSeeder.cs`
  - Donnees categories, tags, posts, events, team, etc. en dur.
- `Program.cs`
  - Execute migration + seed au demarrage:
  - `dbContext.Database.Migrate();`
  - `await DatabaseSeeder.SeedDataAsync(dbContext);`

### C. Clé admin par defaut en dur

- `appsettings.json`
- `appsettings.Development.json`
- `Api/Filters/AuthorizeFilter.cs` (fallback sur `dev-admin-key`)
- `DotnetNiger.Community.http` (header `X-Admin-Key: dev-admin-key`)

### D. Identites techniques simulees dans les controllers

Plusieurs controllers creent des `AuthorId`, `OwnerId`, `CreatedBy`, `UserId` via `Guid.NewGuid()` au moment des POST.
Cela genere des auteurs/proprietaires fictifs et empeche la tracabilite reelle.

Exemples:
- `Api/Controllers/PostsController.cs`
- `Api/Controllers/EventsController.cs`
- `Api/Controllers/ProjectsController.cs`
- `Api/Controllers/ResourcesController.cs`
- `Api/Controllers/CommentsController.cs`

## 3) Sante Technique (Principaux Risques)

### Critique

- 62 fichiers `.cs` vides: forte dette technique, confusion d'architecture, risques de regressions.
  - Exemples: `Api/Middleware/*.cs`, `Api/Filters/CacheFilter.cs`, `Application/Services/*` (plusieurs), `Infrastructure/External/*` (plusieurs), etc.

### Eleve

- Logique metier dupliquee / incoherente:
  - services consolides dans `Application/Services/Services.cs` tout en ayant de nombreux fichiers de services individuels vides.
- Absence de vraie auth/identity integree:
  - usage de GUID aleatoires au lieu d'un utilisateur authentifie.

### Moyen

- Seed auto dans `Program.cs` a chaque demarrage:
  - pratique en dev, a cadrer strictement selon environnement.
- Endpoints utilitaires (`TeamController`, `TestControllers`) a isoler ou supprimer pour production.

### Faible

- Nommage/qualite:
  - route `couranDaga` non explicite,
  - incoherences mineures de messages/accents.

## 4) Actions Recommandees (ordre rapide)

1. Extraire/neutraliser les endpoints statiques (`TeamController`, `TestControllers`) pour la prod.
2. Supprimer la fallback key `dev-admin-key` du filtre et imposer config secrete obligatoire.
3. Remplacer les `Guid.NewGuid()` d'identite par l'utilisateur courant (claims/JWT).
4. Refactoriser l'architecture services:
   - garder soit `Services.cs` central, soit fichiers individuels, pas les deux.
5. Traiter les 62 fichiers vides:
   - supprimer ou implementer progressivement selon priorite.
6. Ajouter un socle de tests (unit + integration API) avant d'elargir l'admin.

## 5) Liste rapide de points hardcodes visibles

- `Api/Controllers/TeamController.cs`
- `Api/Controllers/TestControllers.cs`
- `Infrastructure/Data/Seeds/DatabaseSeeder.cs`
- `Program.cs`
- `Api/Filters/AuthorizeFilter.cs`
- `appsettings.json`
- `appsettings.Development.json`
- `DotnetNiger.Community.http`

## 6) Note sur la correction faite pendant le scan

- Correction compile effectuee:
  - `Application/Services/AdminService.cs`
  - remplacement du comptage sur `Project.Status` (propriete inexistante) par un comptage valide pour restaurer un build sain.
