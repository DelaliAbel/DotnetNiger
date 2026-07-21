# DotnetNiger

Plateforme communautaire pour les développeurs .NET au Niger.

## Structure

| Projet | Role | Framework |
|--------|------|-----------|
| `DotnetNiger.Api` | API ASP.NET Core | net9.0 |
| `DotnetNiger.UI` | Blazor WASM | net8.0 |

## Developpement

```bash
# Lancer le backend
dotnet run --project DotnetNiger.Api

# Lancer le frontend
dotnet run --project DotnetNiger.UI
```

Swagger : `http://localhost:5000/swagger`  
Frontend : `http://localhost:5201`

## Tech

- .NET 9.0 / 8.0
- OpenIddict (OAuth2/OIDC)
- SQL Server + EF Core
- Blazor WebAssembly
