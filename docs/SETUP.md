# Setup

Guide court pour installer et demarrer DotnetNiger.

## Prerequis

- .NET SDK 8.0
- Git
- Docker Desktop (recommande) ou SQL Server 2022 + Redis 7

## Demarrage rapide (Docker)

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
docker-compose up -d
```

Acces:

- http://localhost:5000/swagger
- http://localhost:5075/swagger
- http://localhost:5269/swagger

## Demarrage local

```bash
dotnet restore

cd DotnetNiger.Identity
dotnet ef database update
cd ..\DotnetNiger.Community
dotnet ef database update
cd ..

./run.sh     # Linux/Mac
./run.ps1    # Windows
```

## Configuration

- appsettings.Development.json dans chaque service
- ConnectionStrings pour Identity et Community

### Admin seed (Identity)

Le seed admin est optionnel et ne s'execute qu'une seule fois.

PowerShell:

```powershell
$env:SEED_ADMIN="true"
$env:ADMIN_EMAIL="admin@dotnetniger.com"
$env:ADMIN_PASSWORD="AdminPassword@2006"
$env:ADMIN_USERNAME="admin"
```

Pour desactiver apres creation:

```powershell
$env:SEED_ADMIN="false"
```

### Email provider

Choisir `smtp`, `sendgrid`, ou `mailgun` via la config Email.
Exemples dans [docs/API.md](./API.md).

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
