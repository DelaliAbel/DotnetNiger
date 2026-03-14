# Documentation DotnetNiger

Documentation du projet DotnetNiger — plateforme communautaire microservices .NET 8.

> Dernière mise à jour : **2026-03-14**

## Documents

| Fichier                                              | Description                            |
| ---------------------------------------------------- | -------------------------------------- |
| [SETUP](./SETUP.md)                                  | Installation et démarrage local        |
| [ARCHITECTURE](./ARCHITECTURE.md)                    | Vue des services et sécurité           |
| [API](./API.md)                                      | Endpoints Gateway, Identity, Community |
| [HEALTH_REPORT](./HEALTH_REPORT.md)                  | Santé technique et dette               |
| [Gateway README](../DotnetNiger.Gateway/README.md)   | Routing Ocelot et rate limiting        |
| [Identity README](../DotnetNiger.Identity/README.md) | Exploitation du service Identity       |

## Notes

- Ports: Gateway `5000`, Identity `5075`, Community `5269`
- Versioning backend: `/api/v1/...`
- Exposition Gateway: `/api/...`
