# Security Policy

## Reported Security Vulnerabilities

Si vous découvrez une vulnérabilité de sécurité, veuillez **NE PAS** l'ouvrir en public via une issue GitHub.

À la place, veuillez envoyer un email à [Comming...](mailto:Comming...) avec :

- Description de la vulnérabilité
- Étapes pour reproduire
- Impact potentiel
- Votre suggestion de correction (si disponible)

### Délai de réponse

- Nous accuserons réception dans les **24 heures**
- Une évaluation initiale sera fournie dans les **48 heures**
- Nous travaillerons à une correction et la fournirons dès que possible

## Soutien aux versions

| Version | Supported |
|---------|-----------|
| 1.0.x | ✅ Yes |
| < 1.0 | ❌ No |

## Dépendances de sécurité

Les dépendances NuGet sont mises à jour régulièrement pour les correctifs de sécurité :

```bash
dotnet restore
dotnet outdated --include-prerelease
dotnet nuget locals all --clear
```

## Scans de sécurité

DotnetNiger utilise plusieurs outils de sécurité :

- **Dependabot** - Scans de dépendances
- **SonarQube** - Analyse statique
- **OWASP** - Vulnérabilités connues
- **CodeQL** - Analyse du code

## Bonnes pratiques

### Authentification
- Utiliser HTTPS en production
- Valider tous les JWT tokens
- Implémenter rate limiting

### Données
- Chiffrer les données sensibles en transit et au repos
- Valider toutes les entrées utilisateur
- Utiliser des requêtes paramétrées

### Secrets
- Ne jamais commiter les secrets en Git
- Utiliser un gestionnaire de secrets
- Rotationner régulièrement les clés

---

Merci de votre aide pour garder DotnetNiger sécurisé !
