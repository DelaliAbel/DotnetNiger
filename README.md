# DotnetNiger

Plateforme communautaire pour les développeurs .NET au Niger.

## Structure

| Projet | Role | Framework |
|--------|------|-----------|
| `DotnetNiger.Server` | API ASP.NET Core | net9.0 |
| `DotnetNiger.Client` | Blazor WASM | net8.0 |
| `DotnetNiger.Infrastructure` | Services, EF Core | net9.0 |
| `DotnetNiger.Domain` | Entites, DTOs | net9.0 |

## Developpement

```bash
# Lancer le backend
dotnet run --project DotnetNiger.Server

# Lancer le frontend
dotnet run --project DotnetNiger.Client
```

Swagger : `http://localhost:5000/swagger`  
Frontend : `http://localhost:5201`

## Tech

- .NET 9.0 / 8.0
- OpenIddict (OAuth2/OIDC)
- SQL Server + EF Core
- Blazor WebAssembly
