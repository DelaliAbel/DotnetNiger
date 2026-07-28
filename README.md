# DotnetNiger

Plateforme communautaire pour les développeurs .NET au Niger.

## Structure

```
DotnetNiger.Api/          API ASP.NET Core (net9.0)
├── Api/                   Couche Présentation (Controllers, Middleware, Program.cs)
├── Application/           Couche Application (Services, DTOs, Interfaces)
├── Domain/                Couche Domaine (Entities)
└── Infrastructure/        Couche Infrastructure (Data, Email, Auth)

DotnetNiger.UI/            Blazor WASM (net8.0)
├── Services/
│   ├── Api/               Implémentations HTTP réelles
│   ├── Mock/              Implémentations simulées (développement)
│   ├── App/               Services d'état et toast
│   ├── Auth/              Authentification JWT
│   ├── Contracts/         Interfaces des services
│   └── Browser/           Interop JS
├── Components/
├── Pages/
└── Models/
```

## Développement

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
- JWT + OAuth2 (Google, GitHub, Microsoft)
- SQL Server + EF Core
- Blazor WebAssembly
