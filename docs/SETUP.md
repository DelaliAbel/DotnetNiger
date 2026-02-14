# Setup

Guide court pour installer et demarrer DotnetNiger.

## Prerequis

- .NET SDK 8.0
- Git
- SQLite déjà fournie avec le projet (aucune config). Docker uniquement si vous voulez tester SQL Server/Redis.

## Demarrage rapide (local, DB incluse)

```bash
# 1. Cloner et entrer dans le dossier (repo officiel)
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger

# 2. Vérifier la branche courante (après clone vous êtes sur main)
git branch -a

# 3. Créer et basculer sur dev si elle n'existe pas localement
git checkout -b dev origin/main

# 4. Si dev existe déjà côté remote
git checkout dev
git pull

# 5. Installer les dépendances
dotnet restore

# 6. Lancer (base SQLite déjà prête, aucune migration à faire)
./run.sh       # Linux/Mac (ou Windows via Git Bash)
./run.ps1      # Windows (PowerShell)
```

Acces apres demarrage (ports) :

- Gateway : http://localhost:5000/swagger et http://localhost:5000/health
- Identity : http://localhost:5075/swagger
- Community : http://localhost:5269/swagger

## Variante Docker (SQL Server)

```bash
docker-compose up -d
export ConnectionStrings__DotnetNigerIdentityContextConnection="Server=localhost,1433;Database=DotnetNiger.Identity;User ID=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
$env:ConnectionStrings__DotnetNigerIdentityContextConnection="Server=localhost,1433;Database=DotnetNiger.Identity;User ID=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
dotnet ef database update --project DotnetNiger.Identity
```

## Configuration

- appsettings.Development.json dans chaque service
- ConnectionStrings: SQLite locale par défaut déjà fournie; SQL Server optionnel via Docker si besoin

## Verification

```bash
curl http://localhost:5000/health
```

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
