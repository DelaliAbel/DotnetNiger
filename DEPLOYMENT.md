# Déploiement

À renseigner en production, dans les variables d'environnement du site.

## Base de données

| Clé | Valeur |
|---|---|
| `ConnectionStrings__Default` | chaîne de connexion SQL Server |
| `Database__Provider` | `SqlServer` |
| `Database__Initialization` | `None` |

Les migrations s'appliquent au déploiement, pas au démarrage :

```bash
dotnet ef database update \
  --project src/DotnetNiger.Infrastructure \
  --startup-project src/DotnetNiger.Web
```

## Courriel

Sans ces valeurs, aucune inscription ni aucun rappel d'événement n'est envoyé.

| Clé | Valeur |
|---|---|
| `Email__Host` | serveur SMTP |
| `Email__Port` | `587` |
| `Email__UseSsl` | `true` |
| `Email__UserName` | utilisateur SMTP |
| `Email__Password` | mot de passe SMTP |
| `Email__FromAddress` | adresse expéditeur |
| `Email__FromDisplayName` | `DotnetNiger` |
| `Email__SiteUrl` | `https://votre-domaine` |
| `Email__ContactAddress` | adresse de contact |

## Premier administrateur

À ne renseigner que sur une base neuve, puis à vider.

| Clé | Valeur |
|---|---|
| `AdminBootstrap__Email` | adresse du compte administrateur |
| `AdminBootstrap__Password` | mot de passe initial |
| `AdminBootstrap__DisplayName` | `Admin` |

## Connexion externe

Facultatif. Un fournisseur sans identifiant est ignoré.

| Clé | Valeur |
|---|---|
| `ExternalProviders__Providers__Google__ClientId` | identifiant client |
| `ExternalProviders__Providers__Google__ClientSecret` | secret |
| `ExternalProviders__Providers__Microsoft__ClientId` | identifiant client |
| `ExternalProviders__Providers__Microsoft__ClientSecret` | secret |
| `ExternalProviders__Providers__Microsoft__Tenant` | `common` |
| `ExternalProviders__Providers__GitHub__ClientId` | identifiant client |
| `ExternalProviders__Providers__GitHub__ClientSecret` | secret |

## Déploiement

`--runtime` suit l'offre de l'hébergeur : `win-x86` sur l'offre gratuite,
`win-x64` en Premium.

```bash
npm ci
npm run css:minify

dotnet publish src/DotnetNiger.Web \
  --configuration Release \
  --runtime win-x64 \
  --self-contained false
```

Déposer `app_offline.htm` à la racine du site avant la synchronisation WebDeploy, le
retirer après. Sans lui, IIS verrouille les fichiers et la synchronisation échoue.

## Vérifier

Administration → Paramètres doit indiquer que l'envoi est possible. Sinon, la page
nomme elle-même la clé manquante.