# DotnetNiger

Portail de la communauté .NET du Niger : événements, blog, ressources techniques,
projets open source et partenaires.

## La pile

| Élément | Choix |
|---|---|
| Framework | .NET 10 (`net10.0`) |
| Front | Blazor Web App — rendu serveur statique, îlots interactifs |
| API | contrôleurs MVC sous `/api/…` |
| CSS | Tailwind CSS 4, thème dans `wwwroot/css/design/theme.css` |
| Éditeur | TinyMCE 8 (`license_key: gpl`) |
| Base | SQL Server en production, sélection par configuration |
| Versions | Central Package Management (`Directory.Packages.props`) |

Un seul projet est déployé : `src/DotnetNiger.Web`. Les trois autres projets
(`Application`, `Domain`, `Infrastructure`) sont des bibliothèques.

## Démarrer en local

Le SDK .NET 10 est en `/usr/lib/dotnet/dotnet` ; le `dotnet` du `PATH` peut pointer
sur une installation plus ancienne qui ne voit pas `net10.0`.

```bash
npm install                       # Tailwind et TinyMCE
npm run css:minify                # compile le thème vers wwwroot/css/app.css
/usr/lib/dotnet/dotnet build --no-restore
/usr/lib/dotnet/dotnet run --project src/DotnetNiger.Web
```

Le thème CSS est compilé par npm et **versionné**. Après toute modification de
`theme.css`, relancer `npm run css:minify` avant de compiler : le CSS compilé est
ce que le navigateur reçoit, pas la source.

La base locale est une base jetable. Pour la créer et la remplir :

```bash
/usr/lib/dotnet/dotnet ef database update \
  --project src/DotnetNiger.Infrastructure \
  --startup-project src/DotnetNiger.Web
```

Le premier administrateur vient du bloc `AdminBootstrap` de la configuration. Sans
adresse ni mot de passe, il n'y en a pas.

## L'administration

`/administration`, réservée au rôle `Admin`. La barre latérale couvre tout : contenu
(blog, événements, ressources, projets, partenaires), modération (commentaires,
signalements, messages), communauté (membres, abonnés) et outils (paramètres, profil).

Elle est écrite en Blazor et en HTML/CSS maison, sans bibliothèque d'interface.
Deux règles de lecture y sont constantes :

- **les écritures passent par l'API**, comme sur le site public ;
- **les lectures passent par des services d'administration** côté serveur
  (`Services/Administration`), qui interrogent la base sans faire un aller-retour
  HTTP depuis le navigateur.

### Les entités ne se ressemblent pas

| Entité | Image | Corps rédigé | Statut à la création |
|---|---|---|---|
| Article | couverture **obligatoire** | oui | au choix |
| Événement | couverture **obligatoire** | oui | au choix |
| Ressource | aucune — icône de catégorie | non | publié d'office |
| Projet | logo facultatif | oui | **publié d'office** |
| Partenaire | logo facultatif (initiales sinon) | non | affiché d'office |

Une ressource est un lien, et ce lien est obligatoire : sur le site public elle
n'est qu'une carte, qui mène directement à la ressource.

Le chapô d'un article, d'un événement ou d'un projet n'est jamais saisi : il est
déduit du début de son texte.

### Les images se déposent

Les images de contenu vivent dans la table `ContentImage`, sont déposées par
`POST /api/images` et servies par `GET /api/images/{id}`. L'éditeur de texte a un
bouton de dépôt. Aucune adresse d'image n'est saisie à la main.

## Arborescence

```
src/
  DotnetNiger.Domain/          entités, règles, garde-fous
  DotnetNiger.Application/     ports : persistance, identité, annuaire des membres
  DotnetNiger.Infrastructure/ EF Core, Identity, courriels, file d'attente
  DotnetNiger.Web/             contrôleurs, composants Blazor, services
    Components/Pages/          site public
    Components/Pages/Administration/   administration
    Services/Administration/  lectures de l'administration
    Controller/                API
wwwroot/
  css/design/theme.css         thème source
  css/app.css                  thème compilé (versionné)
```

## Déployer

Voir [DEPLOYMENT.md](DEPLOYMENT.md) : les seules clés à renseigner en production.

## Conventions

Les avertissements sont des erreurs (`TreatWarningsAsErrors`), la documentation XML
est obligatoire sur l'API publique, et le style est appliqué à la compilation par
`.editorconfig`.

Avant une première contribution : [CONTRIBUTING.md](CONTRIBUTING.md).