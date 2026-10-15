# Déploiement

Clés à renseigner dans les variables d'environnement du site.

## Base de données

| Clé | Valeur |
|---|---|
| `ConnectionStrings__Default` | chaîne de connexion SQL Server |
| `Database__Provider` | `SqlServer` |
| `Database__Initialization` | `None` |

## Courriel

Sans ces valeurs, rien n'est envoyé.

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

Base neuve seulement, à vider ensuite.

| Clé | Valeur |
|---|---|
| `AdminBootstrap__Email` | adresse du compte |
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