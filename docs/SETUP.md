# Setup — DotnetNiger

Guide rapide pour installer, configurer et démarrer DotnetNiger avec sécurité centralisée (Gateway).

## Prérequis

- .NET SDK 8.0+
- Git
- SQLite (par défaut, aucune config requise)
- Docker (optionnel, pour SQL Server/Redis)

## Démarrage rapide (mode dev, tout local)

```bash
# 1. Cloner le repo
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger

# 2. Basculer sur la branche dev
git checkout dev
git pull

# 3. Installer les dépendances
dotnet restore

# 4. Lancer (SQLite déjà prête, aucune migration à faire)
./run.sh       # Linux/Mac/WSL
./run.ps1      # Windows (PowerShell)
```

Accès après démarrage :

- Gateway : http://localhost:5000/swagger et http://localhost:5000/health
- Identity : http://localhost:5075/swagger
- Community : http://localhost:5269/swagger

## Variante Docker (SQL Server/Redis)

```bash
docker-compose up -d
# Adapter la variable de connexion si besoin :
export ConnectionStrings__DotnetNigerIdentityContextConnection="Server=localhost,1433;Database=DotnetNiger.Identity;User ID=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
$env:ConnectionStrings__DotnetNigerIdentityContextConnection="Server=localhost,1433;Database=DotnetNiger.Identity;User ID=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
dotnet ef database update --project DotnetNiger.Identity
```

## Configuration & Sécurité

- Modifier `appsettings.Development.json` pour vos secrets locaux (clé JWT, string SQL, etc)
- **Ne jamais commiter de secrets** (utiliser variables d’environnement en prod)
- CORS : Identity accepte uniquement Gateway
- JWT : généré et validé via Gateway
- Rate limiting : actif sur Gateway

## Vérification

```bash
curl http://localhost:5000/health
```

## Bonnes pratiques

- Toujours passer par le Gateway (http://localhost:5000)
- Ne jamais exposer Identity/Community directement
- Respecter la Clean Architecture pour toute contribution

## Depannage rapide

- Port occupe: netstat -ano | findstr :5000
- SQL Server: sqlcmd -S localhost -U sa -P YourPassword123!
- Redis: redis-cli ping

## Regles dev (courtes)

- Controllers minces, logique dans Application
- Valider les inputs, retourner des erreurs claires
- Utiliser async/await partout
- Aucun secret dans le code
- DTOs pour requests et responses
- Travailler sur la branche `dev`, merger vers `main` pour les releases
- Rappel Git rapide: `git status`, `git pull`, `git add .`, `git commit -m "feat: ..."`, `git push origin dev`
