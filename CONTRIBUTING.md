# Contributing

1. Creer une branche depuis `dev`
2. Effectuer les modifications
3. Verifier que la solution compile sans erreur ni warning :
   ```bash
   dotnet build DotnetNiger.sln
   ```
4. Ouvrir une pull request vers `dev`

## Branches

- `master` : production
- `dev` : integration
- `feature/*`, `fix/*` : branches de travail

## Conventions

- La logique metier va dans les services, pas les controleurs
- Commits en francais ou anglais
- Pas de commentaires superflus
