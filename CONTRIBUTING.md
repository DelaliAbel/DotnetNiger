# Contributing

Merci de votre intérêt pour DotnetNiger ! Voici comment contribuer au projet.

## Branches

- `master` : production (déploiement automatique)
- `dev` : branche d'intégration (PR vers `master`)
- `feature/*` : nouvelles fonctionnalités (PR vers `dev`)
- `fix/*` : corrections de bugs (PR vers `dev`)
- `refactor/*` : refactoring (PR vers `dev`)

## Workflow

1. Créer une branche depuis `dev` :
   ```bash
   git checkout dev
   git pull origin dev
   git checkout -b feature/ma-feature
   ```
2. Effectuer les modifications
3. Vérifier que la solution compile sans erreur ni warning :
   ```bash
   dotnet build DotnetNiger.sln
   ```
4. Tester les changements :
   ```bash
   dotnet run --project DotnetNiger.Api
   # et dans un autre terminal
   dotnet run --project DotnetNiger.UI
   ```
5. Committer avec des messages clairs (français ou anglais)
6. Pousser la branche et ouvrir une Pull Request vers `dev`

## Conventions de code

- **Style** : Suivre les règles définies dans `.editorconfig` et `.code-workspace`
- **Architecture** : Respecter l'architecture Onion (Domain → Application → Infrastructure → Api)
- **Noms** : PascalCase pour les classes/méthodes, camelCase pour les paramètres/variables locales
- **Services** : La logique métier va dans les services (Application/), pas dans les contrôleurs
- **DTOs** : Toujours utiliser des DTOs pour les entrées/sorties des API
- **Commentaires** : Pas de commentaires superflus — le code doit être auto-documenté
- **Formatage** : Exécuter `dotnet format` avant de commit

## Tests

- Tests unitaires dans `DotnetNiger.Api.Tests/` (si présent)
- Tester manuellement les endpoints via Swagger avant d'ouvrir une PR
- Vérifier les cas limites (données manquantes, utilisateurs non authentifiés, etc.)

## Pull Request

- Titre clair décrivant le changement
- Description des modifications et de leur raison d'être
- Mentionner les issues liées (fixes #123)
- Ajouter des captures d'écran si l'UI est modifiée
- S'assurer que la CI passe (GitHub Actions)

## Commit messages

- Format : `<type>: <description courte>`
- Types : `feat`, `fix`, `refactor`, `docs`, `style`, `test`, `chore`, `clean`
- Exemples :
  - `feat: ajout du système de notification`
  - `fix: correction du double appel API sur la page event`
  - `refactor: extraction du service EmailValidator`
  - `docs: mise à jour du README avec les prérequis`

## Signalement de bugs

Ouvrir une issue GitHub avec :
- Description du problème
- Étapes pour reproduire
- Comportement attendu vs réel
- Environnement (OS, navigateur, version .NET)
- Logs d'erreur (le cas échéant)
