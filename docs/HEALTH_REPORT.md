# Health Report - DotnetNiger

Rapport de sante technique consolide du projet.

## Date de verification

- 2026-03-07

## Etat des services

- Gateway (`DotnetNiger.Gateway`): ✅ Operationnel (Ocelot)
- Identity (`DotnetNiger.Identity`): ✅ Operationnel
- Community (`DotnetNiger.Community`): ⚠️ Partiellement implemente (developpement en cours)

## Etat architecture et routing

- Gateway actif: Ocelot (`DotnetNiger.Gateway/ocelot.json`)
- Ports cibles:
  - Gateway: `5000`
  - Identity: `5075`
  - Community: `5269`
- Scripts d'execution:
  - `run.sh` est le point d'entree unique
  - `init-shared-db.sh` et `start-all-services.sh` ont ete retires

## Securite

- JWT Bearer configure au niveau gateway et services: ✅
- CORS: configure (a restreindre par environnement production): ⚠️
- Rate limiting via Ocelot: ✅
- Secrets via configuration/env vars: ✅ (pas de hardcode recommande)

## Observabilite

- Logs applicatifs via Serilog: ✅
- Correlation (`X-Request-ID`) cote gateway: ✅
- Endpoints de sante: ✅ (selon routes exposees)

## Tests

- Identity unit tests: ⚠️ a stabiliser
- Identity integration tests: ⚠️ partiels
- Gateway tests: ⚠️ a completer
- Community tests: ⚠️ a completer

## Risques/points de vigilance

- Community encore incomplet (services/repositories/controllers a finaliser).
- Stabiliser et augmenter la couverture de tests.
- Finaliser politiques de securite production (CORS strict, verification anti-abus).

## Resume executif

- La base microservices fonctionne avec Ocelot comme gateway central.
- L'axe prioritaire reste la completion fonctionnelle de Community et la fiabilisation des tests.
