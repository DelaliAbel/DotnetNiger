# DotnetNiger Gateway Setup (Ocelot)

Guide de configuration du gateway actuel `DotnetNiger.Gateway`.

## Composants

- Gateway: Ocelot
- Swagger aggregation: MMLib.SwaggerForOcelot
- Port gateway: `5000`
- Downstream Identity: `5075`
- Downstream Community: `5269`

## Fichiers principaux

- `DotnetNiger.Gateway/Program.cs`
- `DotnetNiger.Gateway/ocelot.json`
- `DotnetNiger.Gateway/appsettings.json`
- `DotnetNiger.Gateway/appsettings.Development.json`

## Lancement

```bash
cd DotnetNiger.Gateway
dotnet restore
dotnet run
```

## Endpoints utiles

- `http://localhost:5000/swagger`
- `http://localhost:5000/health`
- `http://localhost:5000/info`

## Notes Ocelot natives

- Le routage est centralise dans `ocelot.json` (`Routes`).
- Les routes protegees utilisent `AuthenticationOptions` avec `Bearer`.
- Le rate limit est configure via `RateLimitOptions`.
- La resilience est configuree via `QoSOptions`.
- Le cache gateway est configure via `FileCacheOptions` et `Ocelot.Cache.CacheManager`.

## Verification rapide

1. Demarrer Identity et Community.
2. Ouvrir `http://localhost:5000/swagger`.
3. Verifier que `identity` et `community` remontent dans Swagger aggregate.
