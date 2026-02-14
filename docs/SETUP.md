# Setup

Guide court pour installer et demarrer DotnetNiger.

## Prerequis

- .NET SDK 8.0
- Git
- **Aucun SGBD externe requis en local** : EF Core crée des fichiers SQLite (Identity + Community) dans `Infrastructure/Data`
- (Optionnel) Docker Desktop + SQL Server 2022 + Redis 7 si vous souhaitez reproduire l'environnement de production

## Demarrage rapide (SQLite locale par defaut)

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
dotnet restore

cd DotnetNiger.Identity
dotnet ef database update   # cree DotnetNigerIdentityDb.db (SQLite)
cd ..\DotnetNiger.Community
dotnet ef database update   # cree CommunityDb.db (SQLite)
cd ..

./run.sh     # Linux/Mac
./run.ps1    # Windows
```

Acces:

- http://localhost:5000/swagger
- http://localhost:5075/swagger
- http://localhost:5269/swagger

Supprimer les fichiers `*.db` pour repartir d'une base propre. Chaque service gère son fichier SQLite, aucune instance SQL Server n'est lancée.

## Demarrage via Docker (optionnel)

```bash
docker-compose up -d    # SqlServer + Redis + services si vous avez configure les connexions
```

Vous pouvez aussi ne lancer que les dependances:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
	-p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
docker run -p 6379:6379 -d redis:7
```

Pensez ensuite a mettre a jour les `ConnectionStrings` pour basculer de SQLite vers SQL Server.

## Configuration

- appsettings.Development.json dans chaque service
- `DotnetNiger.Identity` et `DotnetNiger.Community` pointent sur SQLite (fichiers `.db`).
- Pour utiliser SQL Server/PosgreSQL, editez `ConnectionStrings` ou utilisez des variables d'environnement.

### Secrets (Identity)

Eviter les secrets dans les fichiers. Utiliser des variables d'environnement ou user-secrets.

Exemples:

```bash
dotnet user-secrets set "Jwt:Key" "<jwt-secret>" --project DotnetNiger.Identity
dotnet user-secrets set "Email:Smtp:Password" "<smtp-password>" --project DotnetNiger.Identity
```

Equivalent env vars:

```bash
Jwt__Key=<jwt-secret>
Email__Smtp__Password=<smtp-password>
```

### File upload (avatar)

Le service Identity supporte 2 providers:

- `Local` (par defaut): stockage local et publication via `/uploads`
- `Azure`: stockage via Azure Blob Storage

Si `Provider = Local`, les fichiers sont servis par Identity sur `/uploads` et proxifies par le Gateway (meme chemin).

Le job de cleanup supprime les avatars orphelins (non references en base) selon la frequence definie par `CleanupIntervalMinutes`.

Exemple (Local):

```json
"FileUpload": {
	"Provider": "Local",
	"RootPath": "uploads",
	"PublicBasePath": "/uploads",
	"MaxAvatarBytes": 2000000,
	"AllowedAvatarContentTypes": ["image/jpeg", "image/png", "image/webp"],
	"AllowedAvatarExtensions": [".jpg", ".jpeg", ".png", ".webp"],
	"CleanupEnabled": false,
	"CleanupIntervalMinutes": 1440,
	"CleanupOrphanDays": 7,
	"Azure": {
		"ConnectionString": "",
		"Container": "dotnetniger-uploads",
		"PublicBaseUrl": ""
	}
}
```

Exemple (Azure):

```json
"FileUpload": {
	"Provider": "Azure",
	"Azure": {
		"ConnectionString": "<azure-connection-string>",
		"Container": "dotnetniger-uploads",
		"PublicBaseUrl": ""
	}
}
```

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
