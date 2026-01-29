# Script pour effectuer le commit initial et push vers GitHub
# Usage: .\scripts\push-initial.ps1

param(
    [string]$Branch = "main",
    [string]$RemoteUrl = "",
    [switch]$DryRun
)

Write-Host "🚀 Initialisation du push GitHub" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Split-Path -Parent $PSScriptRoot

# 1. Vérification finale
Write-Host "1️⃣  Exécution de la vérification finale..." -ForegroundColor Yellow
& "$projectRoot\scripts\verify-clean.ps1"

Write-Host ""
Write-Host "2️⃣  Préparation du commit..." -ForegroundColor Yellow

# 2. Add all files
if ($DryRun) {
    Write-Host "  [DRY-RUN] git add ."
} else {
    git -C $projectRoot add .
    Write-Host "  ✅ Fichiers stagés"
}

# 3. Commit avec message structuré (Conventional Commits)
$commitMessage = @"
feat: Initial project setup with complete documentation and CI/CD

- Add comprehensive project documentation (README, ARCHITECTURE, API, SETUP, DEPLOYMENT, etc.)
- Configure GitHub Actions CI/CD pipelines (tests, docker, sonar, deploy)
- Set up project structure with clean architecture layers
- Add configuration files (.gitignore, .editorconfig, .gitattributes)
- Create issue and pull request templates
- Set up environment configuration files for all services
- Add CODEOWNERS and collaboration guidelines
- Prepare Docker infrastructure and deployment configurations
"@

Write-Host "3️⃣  Création du commit..." -ForegroundColor Yellow

if ($DryRun) {
    Write-Host "  [DRY-RUN] git commit -m `"$commitMessage`""
} else {
    git -C $projectRoot commit -m $commitMessage
    Write-Host "  ✅ Commit effectué"
}

# 4. Configuration du remote (si fourni)
if ($RemoteUrl) {
    Write-Host "4️⃣  Configuration du remote..." -ForegroundColor Yellow
    if ($DryRun) {
        Write-Host "  [DRY-RUN] git remote add origin $RemoteUrl"
    } else {
        git -C $projectRoot remote add origin $RemoteUrl 2>$null
        Write-Host "  ✅ Remote configuré"
    }
}

# 5. Push
Write-Host "5️⃣  Push vers GitHub..." -ForegroundColor Yellow

if ($DryRun) {
    Write-Host "  [DRY-RUN] git push -u origin $Branch"
} else {
    git -C $projectRoot push -u origin $Branch
    Write-Host "  ✅ Push effectué"
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Projet poussé vers GitHub!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host ""
Write-Host "📋 Prochaines étapes:" -ForegroundColor Cyan
Write-Host "  1. Configurer les branch protection rules sur GitHub"
Write-Host "  2. Configurer les secrets GitHub (API keys, tokens, etc.)"
Write-Host "  3. Configurer les webhooks pour SonarCloud"
Write-Host "  4. Configurer les environnements de déploiement"
Write-Host "  5. Inviter les collaborateurs"
