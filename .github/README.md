# .github/ - Structure et organisation

Structure complète du répertoire `.github/` pour DotnetNiger.

## 📂 Structure

```
.github/
├── CODEOWNERS                    # Propriétaires du code par section
│
├── 📁 ISSUE_TEMPLATE/
│   ├── bug_report.md            # Template pour signaler les bugs
│   ├── feature_request.md       # Template pour les demandes de features
│   └── config.yml               # Configuration des issues
│
├── 📁 workflows/                # GitHub Actions
│   ├── tests.yml                # Tests & qualité du code
│   ├── docker.yml               # Build & push Docker images
│   ├── sonar.yml                # Analyse SonarCloud
│   ├── deploy.yml               # Déploiement en production
│   └── ci.yml                   # Pipeline CI continu
│
└── PULL_REQUEST_TEMPLATE.md     # Template pour les PRs
```

## 📋 Fichiers détaillés

### CODEOWNERS
Définit les propriétaires du code par section :
- `@dotnetniger-team` - Propriétaires par défaut
- `@gateway-team` - Gateway service
- `@identity-team` - Identity service
- `@community-team` - Community service
- `@devops-team` - Infrastructure & deployment

Les PRs modifiant ces sections nécessitent leur approbation.

### ISSUE_TEMPLATE/

#### bug_report.md
Template pour les bug reports avec :
- Description du bug
- Étapes pour reproduire
- Comportement attendu vs actuel
- Environnement
- Logs/Stack trace
- Priorité suggérée

#### feature_request.md
Template pour les demandes de features avec :
- Description de la fonctionnalité
- Problème qu'elle résout
- Solutions proposées
- Alternatives considérées
- Enjeux & effort estimé
- Acceptance criteria

#### config.yml
Configuration GitHub Issues :
- `blank_issues_enabled: false` - Forcer l'utilisation des templates
- Liens utiles pour le support et les problèmes de sécurité

### PULL_REQUEST_TEMPLATE.md
Template pour les PRs guidant les contributeurs :
- Description du changement
- Type de changement (bug, feature, docs, etc.)
- Tests effectués
- Checklist qualité
- Liens aux issues

### workflows/

#### tests.yml
**Déclenchement:** Chaque push/PR sur main et develop

**Actions:**
- Build du projet
- Tests unitaires et d'intégration
- Rapports de couverture
- Vérification des erreurs/warnings

**Services utilisés:**
- SQL Server (1433)
- Redis (6379)
- Codecov (upload coverage)

#### docker.yml
**Déclenchement:** Chaque push sur main et tags v*

**Actions:**
- Build des images Docker
- Push vers GitHub Container Registry
- Scan de sécurité Trivy
- Upload des résultats

**Services:**
- Docker Buildx
- GitHub Container Registry

#### sonar.yml
**Déclenchement:** Chaque push/PR sur main et develop

**Actions:**
- Analyse SonarCloud
- Rapports de couverture
- Vérification du style de code
- Formatage du code

**Services:**
- SonarCloud
- dotnet-sonarscanner

#### deploy.yml
**Déclenchement:** Chaque tag v*

**Actions:**
- Extraction de la version
- Création de GitHub Release
- Déploiement vers Azure
- Notifications de déploiement

**Secrets requis:**
- `AZURE_CREDENTIALS` - Credentials Azure

#### ci.yml
**Déclenchement:** Chaque push/PR

**Actions:**
- Build continu
- Linting
- Tests rapides

## 🔐 Secrets requis

Pour que les workflows fonctionnent, configurez ces secrets dans GitHub :

```
SONAR_TOKEN              # Token SonarCloud
AZURE_CREDENTIALS        # Credentials Azure
GITHUB_TOKEN             # Token GitHub (auto)
```

Accédez à : Settings → Secrets and variables → Actions

## ✅ Configuration GitHub

### Branch Protection Rules

Pour `main` et `develop` :
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
- ✅ Require code reviews (minimum 2)
- ✅ Require branches to be up to date before merging
- ✅ Require review from Code Owners

### Settings

- ✅ Allow auto-merge
- ✅ Automatically delete head branches
- ✅ Allow squash merging
- ✅ Allow rebase merging
- ✅ Allow force pushes (pour admins seulement)

## 🚀 Utilisation

### Signaler un bug
1. Aller à Issues → New Issue
2. Sélectionner "Bug report"
3. Remplir le template
4. Ajouter le label `bug`

### Demander une feature
1. Aller à Issues → New Issue
2. Sélectionner "Feature request"
3. Remplir le template
4. Ajouter le label `enhancement`

### Soumettre une PR
1. Fork le repo
2. Créer une branche (`git checkout -b feature/name`)
3. Commit les changements
4. Push vers votre fork
5. Créer une PR
6. Le template s'affiche automatiquement
7. Les workflows s'exécutent

## 📊 Statuts des workflows

| Workflow | Badge |
|----------|-------|
| Tests | [![Tests](../../actions/workflows/tests.yml/badge.svg)](../../actions/workflows/tests.yml) |
| Docker | [![Docker](../../actions/workflows/docker.yml/badge.svg)](../../actions/workflows/docker.yml) |
| Sonar | [![Sonar](../../actions/workflows/sonar.yml/badge.svg)](../../actions/workflows/sonar.yml) |
| Deploy | [![Deploy](../../actions/workflows/deploy.yml/badge.svg)](../../actions/workflows/deploy.yml) |

## 🔧 Maintenance

### Ajouter un workflow
1. Créer un fichier `.yml` dans `workflows/`
2. Définir les déclencheurs (`on:`)
3. Définir les jobs
4. Tester via Actions

### Mettre à jour CODEOWNERS
```
# Format
path/to/directory @username @team
```

### Secrets
- Ne jamais commiter les secrets
- Utiliser GitHub Secrets uniquement
- Rotationner régulièrement

## 📚 Ressources

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [Workflow Syntax](https://docs.github.com/actions/using-workflows/workflow-syntax-for-github-actions)
- [Events that trigger workflows](https://docs.github.com/actions/using-workflows/events-that-trigger-workflows)

---

**Organisation finale du .github/**
- ✅ Templates configurés
- ✅ Workflows en place
- ✅ CODEOWNERS défini
- ✅ Prêt pour la collaboration !
