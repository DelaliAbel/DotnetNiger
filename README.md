# DotnetNiger

Plateforme communautaire pour les développeurs .NET au Niger.  
Elle permet aux membres de partager des articles, projets, événements, ressources, et de collaborer au sein d'une communauté tech dynamique.

---

## Fonctionnalités

- **Blog** — Articles techniques avec catégories et tags
- **Événements** — Création, inscription, suivi des participants
- **Projets** — Showcase des projets open-source des membres
- **Ressources** — Partages de ressources (vidéos, livres, outils)
- **Annuaire** — Profils membres avec rôles (Admin, Collaborateur, Membre)
- **Partenaires** — Gestion des sponsors et partenaires
- **Newsletter** — Inscription et envoi de newsletters
- **Certificats** — Génération et gestion de certificats
- **Messagerie** — Système de contact et notifications
- **Authentification** — JWT + OAuth2 (Google, GitHub, Microsoft)

---

## Architecture

```
DotnetNiger.Api/          API ASP.NET Core (net9.0) — Architecture Onion
├── Api/                   Couche Présentation (Controllers, Middleware, Program.cs)
├── Application/           Couche Application (Services, DTOs, Interfaces)
├── Domain/                Couche Domaine (Entities, Value Objects)
└── Infrastructure/        Couche Infrastructure (Data/EF Core, Email, Auth)

DotnetNiger.UI/            Blazor WebAssembly (net8.0)
├── Services/
│   ├── Api/               Implémentations HTTP réelles
│   ├── Mock/              Implémentations simulées (développement local)
│   ├── App/               Services d'état partagé (Toast, Confirmation, Thème)
│   ├── Auth/              Authentification JWT côté client
│   ├── Contracts/         Interfaces des services
│   └── Browser/           Interop JavaScript
├── Components/            Composants réutilisables Blazor
├── Pages/                 Pages de l'application
└── Models/                Modèles côté client
```

---

## Prérequis

- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (pour le frontend Blazor WASM)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (local ou distant)
- [Node.js](https://nodejs.org/) (pour Tailwind CSS)
- Visual Studio 2022+, VS Code, ou Rider

---

## Développement local

```bash
# 1. Restaurer les dépendances
dotnet restore DotnetNiger.sln

# 2. Configurer la base de données
#    - Éditer DotnetNiger.Api/appsettings.Development.json
#    - Ou définir ConnectionStrings__DefaultConnection en variable d'environnement

# 3. Appliquer les migrations
dotnet ef database update --project DotnetNiger.Api

# 4. Lancer le backend (port 5000)
dotnet run --project DotnetNiger.Api

# 5. (Dans un autre terminal) Lancer le frontend (port 5201)
dotnet run --project DotnetNiger.UI
```

**Swagger** : `http://localhost:5000/swagger`  
**Frontend** : `http://localhost:5201`

---

## Variables d'environnement

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Chaîne de connexion SQL Server |
| `Jwt__SecretKey` | Clé secrète JWT (32+ caractères) |
| `Jwt__Issuer` | Émetteur JWT |
| `Jwt__Audience` | Audience JWT |
| `Smtp__Host` | Serveur SMTP |
| `Smtp__Port` | Port SMTP |
| `Smtp__Username` | Utilisateur SMTP |
| `Smtp__Password` | Mot de passe SMTP |
| `Cors__AllowedOrigins` | Domaines autorisés par CORS (séparés par des virgules) |
| `Authentication__Google__ClientId` | Google OAuth Client ID |
| `Authentication__Google__ClientSecret` | Google OAuth Client Secret |
| `Authentication__GitHub__ClientId` | GitHub OAuth Client ID |
| `Authentication__GitHub__ClientSecret` | GitHub OAuth Client Secret |
| `Authentication__Microsoft__ClientId` | Microsoft OAuth Client ID |
| `Authentication__Microsoft__ClientSecret` | Microsoft OAuth Client Secret |
| `ASPNETCORE_ENVIRONMENT` | `Development` ou `Production` |

---

## Déploiement

Voir [DEPLOY_PROD.md](./DEPLOY_PROD.md) pour les instructions complètes de déploiement sur Monster ASP avec :
- Migration de base de données sans perte
- Sauvegarde et restauration des uploads
- CI/CD avec GitHub Actions
- Procédure de rollback

---

## Tech Stack

| Couche | Technologie |
|--------|-------------|
| Backend | ASP.NET Core 9.0, EF Core 9.0 |
| Frontend | Blazor WebAssembly 8.0 |
| Base de données | SQL Server |
| Authentification | JWT + OAuth2 (Google, GitHub, Microsoft) |
| CSS | Tailwind CSS |
| Email | SMTP (Gmail) |
| CI/CD | GitHub Actions |
| Hébergement | Monster ASP (API) / Vercel, Netlify (UI) |

---

## Licence

[MIT](./LICENSE.md)
