# Health Report — DotnetNiger

Rapport de santé technique consolidé du projet.

> Dernière vérification : **2026-03-11**

## État des services

| Service | Statut | Notes |
|---------|--------|-------|
| Gateway (`DotnetNiger.Gateway`) | ✅ Opérationnel | Ocelot + Polly QoS actif |
| Identity (`DotnetNiger.Identity`) | ✅ Opérationnel | JWT + API Key, versioning v1 |
| Community (`DotnetNiger.Community`) | ✅ Opérationnel | Versioning v1 ajouté, CRUD complet |

## État architecture et routing

- Gateway actif : Ocelot (`DotnetNiger.Gateway/ocelot.json`)
- QoS / Circuit breaker : `Ocelot.Provider.Polly` 24.1.0 (requis — sans lui, erreur `QosDelegatingHandlerDelegate not registered`)
- Ports :
  - Gateway : `5000`
  - Identity : `5075`
  - Community : `5269`
- Base de données : SQLite partagée `../DotnetNiger.db`
- Scripts d'exécution : `run.sh` (point d'entrée unique)
- Routes Community : toutes versionnées `api/v{version:apiVersion}/...` → downstream `/api/v1/...`

## Sécurité

| Mécanisme | Statut | Notes |
|-----------|--------|-------|
| JWT Bearer (Gateway + Identity + Community) | ✅ | Clé unifiée sur les 3 services |
| API Key (Identity) | ✅ | Header `X-API-Key` |
| Admin Community (`X-Admin-Key` + `X-Admin-Role`) | ✅ Dev | Clé à remplacer en production via `Admin__ApiKey` env var |
| Rate limiting (Ocelot) | ✅ | Par route dans `ocelot.json` |
| CORS | ⚠️ | À restreindre en production |
| Secrets | ✅ | Via config / env vars — aucun secret hardcodé |

## Observabilité

- Logs via Serilog (fichiers rotatifs dans `logs/`) : ✅
- Correlation ID (`X-Request-ID`) côté Gateway : ✅
- Endpoints de santé (`/health`) : ✅
- Swagger par service + agrégé Gateway : ✅

## Tests

| Suite | Statut | Détails |
|-------|--------|---------|
| Identity unit tests | ✅ 7/7 passants | `UsersControllerTests.cs` |
| Identity integration tests | ⚠️ Partiels | `AdminEndpointsTests`, `DiagnosticsEndpointsTests` |
| Gateway tests | ⚠️ Absent | À compléter |
| Community tests | ⚠️ Absent | À compléter |

## Risques et points de vigilance

- **Admin Community** : `Admin__ApiKey` de production doit être défini via variable d'environnement (jamais committé dans un fichier de config).
- **JWT Key** : La clé de développement `DevOnly_...` doit être remplacée par une clé forte en production.
- **CORS** : Politique à restreindre aux origines autorisées en production.
- **Member feature** (Community) : Désactivée, à activer quand le domaine sera finalisé.
- **Couverture de tests** : Gateway et Community sans tests — à prioriser.

## Résumé exécutif

La base microservices est fonctionnelle et stable pour le développement.
Les trois services démarrent, communiquent via le Gateway Ocelot, et partagent la même clé JWT.
L'axe prioritaire est la sécurisation pour la production (variables d'env, CORS strict) et l'augmentation de la couverture de tests.
