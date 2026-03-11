# Documentation DotnetNiger

Documentation du projet DotnetNiger — plateforme communautaire microservices .NET 8.

> Dernière mise à jour : **2026-03-11**

## Documents

| Fichier | Description |
|---------|-------------|
| [SETUP](./SETUP.md) | Installation et démarrage local |
| [ARCHITECTURE](./ARCHITECTURE.md) | Vue d'ensemble des services et modèle de sécurité |
| [API](./API.md) | Référence complète des endpoints (Identity + Community + Gateway) |
| [HEALTH_REPORT](./HEALTH_REPORT.md) | Santé technique, dette et prochaines étapes |
| [BLAZOR_WASM_INTEGRATION](./BLAZOR_WASM_INTEGRATION.md) | Guide interconnexion frontend Blazor WASM |
| [Gateway README](../DotnetNiger.Gateway/README.md) | Configuration Ocelot |
| [Identity README](../DotnetNiger.Identity/README.md) | Guide du service Identity |
| [Community Health](../DotnetNiger.Community/PROJECT_HEALTH_RECAP.md) | Santé du service Community |

## Parcours rapides

- **Nouveau développeur** : [SETUP](./SETUP.md) → [API](./API.md)
- **Comprendre l'architecture** : [ARCHITECTURE](./ARCHITECTURE.md)
- **Connecter un frontend Blazor WASM** : [BLAZOR_WASM_INTEGRATION](./BLAZOR_WASM_INTEGRATION.md)
- **État du projet** : [HEALTH_REPORT](./HEALTH_REPORT.md)

## Notes clés

- **Ports** : Gateway `5000` · Identity `5075` · Community `5269`
- **Versioning** : toutes les routes sont préfixées `/api/v1/...` (Identity et Community)
- **Base de données** : SQLite partagé (`../DotnetNiger.db`) — aucun SQL Server/Docker requis en dev
- **Clé JWT** : identique sur les 3 services (`Jwt:Key` dans chaque `appsettings.json`)
- **Admin Community** : headers `X-Admin-Key` + `X-Admin-Role` obligatoires (+ JWT Bearer via Gateway)
- **Secrets** : ne jamais committer de vraies clés — utiliser variables d'environnement en production
- **Feature désactivée** : Member (controller/service/repo commentés dans Community)
- **Tests** : 7/7 Identity unit tests passants
