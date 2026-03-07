# Setup DotnetNiger

Guide court pour demarrer le projet en local avec le gateway Ocelot.

## Prerequis

- .NET SDK 8.0+
- Git
- PowerShell (Windows) ou bash (Linux/Mac)

## 1. Recuperer le code

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
```

## 2. Restaurer les packages

```bash
dotnet restore
```

## 3. Lancer les services

Option scripts projet:

```bash
./run.sh
# ou
./run.ps1
```

Option manuelle (3 terminaux):

```bash
cd DotnetNiger.Identity && dotnet run
cd DotnetNiger.Community && dotnet run
cd DotnetNiger.Gateway && dotnet run
```

## 4. Verifier les endpoints

- Gateway Swagger: `http://localhost:5000/swagger`
- Gateway health: `http://localhost:5000/health`
- Identity Swagger: `http://localhost:5075/swagger`
- Community Swagger: `http://localhost:5269/swagger`

## 5. Tester via Gateway

```bash
curl -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"test@example.com\",\"password\":\"Test@123\"}"
```

## Configuration utile

- `DotnetNiger.Gateway/ocelot.json`: routes Ocelot, auth, rate limit, qos, cache, swagger aggregation
- `DotnetNiger.Gateway/appsettings*.json`: logs + JWT settings

Variables JWT (optionnel, recommande hors dev):

```bash
Jwt__Key=your_very_long_secret_key_min_32_chars
Jwt__Issuer=DotnetNiger.Identity
Jwt__Audience=DotnetNiger.Identity.Client
```

## Depannage

1. Gateway build OK mais swagger aggregate KO

- Verifier que les downstream services tournent bien sur `5075` et `5269`.

2. Reponse 401 sur routes protegees

- Verifier `Authorization: Bearer <token>`.
- Verifier coherence `Jwt__Key` entre Identity et Gateway.

3. Reponse 429 (too many requests)

- Limites appliquees par Ocelot (`RateLimitOptions` dans `ocelot.json`).
