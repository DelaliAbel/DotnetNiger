# Setup DotnetNiger

Guide de démarrage local avec Gateway Ocelot.

> Dernière mise à jour : **2026-03-14**

## Prérequis

- .NET SDK 8.0+
- Git
- PowerShell (Windows) ou bash (Linux/Mac)

## 1. Récupérer le code

```bash
git clone https://github.com/DelaliAbel/DotnetNiger.git
cd DotnetNiger
```

## 2. Restaurer les packages

```bash
dotnet restore
```

## 3. Lancer les services

Ordre obligatoire : **Identity → Community → Gateway**

```bash
# Terminal 1
cd DotnetNiger.Identity  && dotnet run

# Terminal 2
cd DotnetNiger.Community && dotnet run

# Terminal 3
cd DotnetNiger.Gateway   && dotnet run
```

## 4. Vérifier les endpoints

| URL                             | Description            |
| ------------------------------- | ---------------------- |
| `http://localhost:5000/swagger` | Swagger agrégé Gateway |
| `http://localhost:5000/health`  | Health check Gateway   |
| `http://localhost:5075/swagger` | Swagger Identity       |
| `http://localhost:5269/swagger` | Swagger Community      |

## 5. Tester via Gateway

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin@123"}'

# Appel protégé
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer <token>"

# Endpoint Community (public)
curl http://localhost:5000/api/posts
```

## 6. Configuration Admin Community

Les endpoints `/api/v1/admin/*` de Community nécessitent deux headers supplémentaires :

```bash
curl -X GET http://localhost:5269/api/v1/admin/dashboard \
  -H "Authorization: Bearer <jwt_token>" \
  -H "X-Admin-Key: dev-community-admin-key-change-me" \
  -H "X-Admin-Role: admin"
```

La clé est définie dans `DotnetNiger.Community/appsettings.Development.json` → `Admin:ApiKey`.

> **Production** : définir via variable d'environnement `Admin__ApiKey` ou `dotnet user-secrets`.

## 7. Variables d'environnement clés

```bash
# Clé JWT — MÊME valeur sur les 3 services
Jwt__Key=your_very_long_secret_key_min_32_chars
Jwt__Issuer=DotnetNiger.Identity
Jwt__Audience=DotnetNiger.Identity.Client

# Clé admin Community
Admin__ApiKey=your_strong_admin_key
```

## Dépannage

### Gateway crash au démarrage — `QosDelegatingHandlerDelegate not registered`

S'assurer que `Ocelot.Provider.Polly` est installé et `.AddPolly()` appelé dans `Program.cs`.

```bash
dotnet add package Ocelot.Provider.Polly --version 24.1.0
```

### Gateway build OK mais Swagger agrégé KO

Vérifier que Identity (`:5075`) et Community (`:5269`) tournent avant de démarrer le Gateway.

### Réponse 401 sur routes protégées

- Vérifier `Authorization: Bearer <token>`.
- Vérifier que `Jwt__Key` est identique sur Identity, Community et Gateway.

### Réponse 429 (Too Many Requests)

Vérifier les `RateLimitOptions` de la route ciblée dans `DotnetNiger.Gateway/ocelot.json`.
Les routes `/health` et `/info` doivent rester avec `EnableRateLimiting = false`.

### Fichier `.exe` verrouillé à la recompilation Community

Un process `DotnetNiger.Community` est déjà en cours. Arrêter le terminal existant avant de relancer `dotnet run`.
