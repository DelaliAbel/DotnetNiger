# dotNetNiger — Récapitulatif des décisions techniques

Statut : validé en discussion avec le porteur du projet.
Dernière mise à jour : 2026-09-27.

Ce document fait foi. Toute décision listée ici a été explicitement validée.

---

## 1. Pile technique

| Élément | Décision | Note |
|---|---|---|
| Framework | .NET 10 (`net10.0`) | SDK 10.0.110 présent sur la machine |
| Front | Blazor Web App, rendu serveur statique + îlots interactifs | SEO, previews réseaux sociaux, rendu HTML complet |
| Backend | API REST sous `/api/…`, en **contrôleurs MVC** (`[ApiController]`) | Style d'écriture tranché : contrôleurs plutôt que minimal API. Voir section 16 |
| Modèle de déploiement | Un seul projet déployé | Voir section 2 pour la justification |
| CSS | **Tailwind CSS 4**, thème déclaré dans `wwwroot/css/design/theme.css` | Compilé par la CLI Tailwind via npm, appelé par une cible MSBuild. Voir section 10 |
| Base de données | **SQLite en développement local**, **SQL Server en production** | Sélection par configuration, voir section 5 |
| Gestion des versions | Central Package Management (`Directory.Packages.props`) | Une seule source de versions pour toute la solution |
| CI | GitHub Actions | |
| Hébergement | MonsterASP.NET | Voir section 3 |

### Détail important sur le SDK

Deux installations de .NET coexistent sur la machine :

- `/home/akaletekoffilevis/.dotnet/dotnet` → SDK 8.0.420 et 9.0.313
- `/usr/lib/dotnet/dotnet` → SDK **10.0.110** (seul à supporter `net10.0`)

Le `dotnet` présent dans le `PATH` pointe vers la première installation et **ne voit pas .NET 10**.
Toutes les commandes de build, de test et de publication doivent donc utiliser
`/usr/lib/dotnet/dotnet`, ou le `PATH` doit être corrigé avant de commencer.

Vérifié : un projet `net10.0` se construit avec 0 avertissement et 0 erreur.

---

## 2. Un seul projet déployé

Décision : **un seul projet** contient les pages publiques, l'API et les tableaux de bord.

Justification, pour une CI/CD propre :

- **Deux déploiements ne peuvent pas être atomiques.** WebDeploy ne gère pas de transaction
  entre deux sites. Il existerait toujours une fenêtre où le front parle à une API d'une
  autre version.
- **Un seul artefact, une seule version, un seul rollback.**
- Pas de CORS, pas de jeton à transporter entre deux origines.
- Budget mémoire : 256 Mo en offre gratuite. Deux applications .NET, c'est deux fois
  les frais du ramasse-miettes.

### Ce que le découpage en projets ne change pas

Séparer le site de l'API **ne protège pas les fichiers uploaded**. Ce qui décide de la
suppression des fichiers, c'est la stratégie de déploiement, pas l'architecture.

Protection retenue contre la perte d'images :

- **Images stockées en base de données**, pas sur le disque. La base survit à tous les
  déploiements, elle est sauvegardée automatiquement chaque jour par l'hébergeur, et
  cela reste cohérent avec les moteurs SQLite, SQL Server et PostgreSQL.
- **WebDeploy en mode `sync`**, jamais `removeUnmatchedFiles`, jamais de
  `dangerous-clean-slate` sur le dossier applicatif.

---

## 3. Hébergement — MonsterASP.NET

### Offre retenue

**Offre gratuite pour les tests.** Premium Single envisagé pour la production.

| | Gratuit | Premium Single |
|---|---|---|
| Prix | 0 | 1,95 $/mois la 1re année, puis 2,50 $ |
| RAM | 256 Mo | 512 Mo |
| Architecture | **win-x86** | **win-x64** |
| Bases | 1 (1 Go) | 2 (2 Go chacune) |
| Stockage | 5 Go | 25 Go |
| Domaine personnalisé | Non | Oui |
| Datacenter | UE | UE ou USA |

Conséquence : le **runtime ID de publication est une variable de configuration**, car il
passe de `win-x86` à `win-x64` selon l'offre. Ce changement ne doit jamais demander de
modifier le code.

### Capacités retenues

- .NET 10, Blazor, SignalR, WebSockets : pris en charge
- SQL Server 2025 et MySQL/MariaDB : pris en charge
- **PostgreSQL : non proposé par cet hébergeur**
- Pas de Docker, pas de Redis, pas de MinIO
- IIS, un seul site servi, pas de proxy inverse configurable

### Contraintes qui ont modifié des décisions antérieures

- **Pas de PostgreSQL** → SQL Server en production.
- **Pas de stockage objet** → les images vivent en base de données.
- **Pas de Redis** → le cache est en mémoire, avec repli sur la base.
- **Mémoire limitée** → le réglage du ramasse-miettes et les pools de connexion
  devront être ajustés explicitement.
- **Pas de tâche de fond fiable** → la newsletter utilise une table d'envoi et une
  tâche planifiée du panneau de contrôle.

### Nom de domaine

Non fourni. L'hébergeur n'est pas registrar, il faudra un registrar tiers
(Cloudflare, Porkbun, Namecheap). Tant que l'offre gratuite est utilisée, le site
n'est accessible que sur un sous-domaine `*.monsterasp.net`.

---

## 4. Intégration continue et livraison

### Pipelines

**Build et vérification** — sur chaque push et chaque demande de fusion :
restauration, compilation, tests unitaires, tests d'intégration, tests d'architecture,
analyse de sécurité des dépendances.

**Livraison** — sur `main`, ou manuellement via `workflow_dispatch` :
publication, arrêt du site, déploiement WebDeploy, migrations, remise en service,
test de fumée.

### Secrets GitHub à configurer

| Secret | Rôle |
|---|---|
| `WEBSITE_NAME` | Identifiant du site, type `siteXXXX` |
| `SERVER_COMPUTER_NAME` | `https://siteXXXX.siteasp.net:8172` |
| `SERVER_USERNAME` | `siteXXXX` |
| `SERVER_PASSWORD` | Mot de passe WebDeploy |
| `CONNECTION_STRING` | Chaîne de connexion SQL Server de production |
| `SMTP_*` | Hôte, port, utilisateur, mot de passe, expéditeur |
| `OAUTH_GOOGLE_*`, `OAUTH_MICROSOFT_*`, `OAUTH_GITHUB_*` | Identifiant client et secret des fournisseurs externes |
| `GITHUB_TOKEN` | Lecture de l'API GitHub pour les contributeurs |

### Points de vigilance

- `app_offline.htm` doit être déposé avant le déploiement et retiré après, sinon IIS
  verrouille les fichiers et la synchronisation échoue.
- Le déploiement provoque **quelques secondes d'arrêt**. Accepté pour ce site.
- La restauration des sauvegardes est quotidienne côté hébergeur, mais un déclencheur
  de sauvegarde dans l'application reste souhaitable.

---

## 5. Base de données

- **SQLite** en développement local, dans `src/DotnetNiger.Web/App_Data/dotnetniger.db`.
  Base jetable, aucun outil à installer, aucun serveur à démarrer.
- **SQL Server** en production, seule base proposée par l'hébergeur.
- **PostgreSQL** : le code est écrit pour pouvoir l'activer, mais ce n'est pas une cible
  de première classe puisque l'hébergeur ne le propose pas. Le provider est déclaré dans
  la configuration, exactement comme dans le template de référence.
- Les migrations sont appliquées automatiquement au déploiement, avec sauvegarde avant
  exécution.

### Stratégie de schéma

Le provider et le mode d'initialisation sont deux réglages distincts
(`Database:Provider` et `Database:Initialization`), pour ne jamais dépendre du nom du
fournisseur pour décider du comportement.

`EnsureCreated` et `Migrate` ne se combinent pas : le premier ignore l'historique des
migrations. Maintenir deux jeux de migrations, un par fournisseur, alourdirait le projet
pour rien, d'autant que la base locale est jetable. La règle retenue est donc :

| Environnement | Provider | Initialisation |
|---|---|---|
| Développement local | SQLite | `EnsureCreated` — le schéma est déduit du modèle |
| Tests | SQLite | `EnsureCreated` — une base neuve par exécution |
| Production | SQL Server | `None` au démarrage, `Migrate` pendant le déploiement |
| Intégration continue | SQL Server | `Migrate`, sur une base dédiée |

Un **unique jeu de migrations**, généré pour SQL Server, qui est la seule cible où les
données survivent. Une base SQLite créée par `EnsureCreated` n'a pas d'historique de
migration, et n'en a pas besoin.

Conséquence pratique : pour redéployer le site en local après un modèle modifié, il faut
supprimer le fichier `.db`. `EnsureCreated` ne le fait pas tout seul.

Vérifié : le schéma s'applique sans erreur sur les deux fournisseurs, et la contrainte
`CK_Comments_ExactlyOneTarget` interdit bien à un commentaire de viser deux cibles à la
fois, ou aucune.

---

## 5 bis. Périmètre des modèles — version 1

| Entité | Décision |
|---|---|
| `Article` | **Distincte** de `Resource` : c'est le blog, rédigé par les admins |
| `Resource` | Distincte, publiable par les membres |
| Commentaires | Autorisés sur les **articles** et les **événements**, pas sur les ressources |
| Cibles du signalement | Commentaires, articles, ressources, événements, projets, membres |
| Partenaires et sponsors | Inclus |
| Messages de contact | **Exclus** de la version 1 |
| Newsletter | **Exclue** de la version 1 (voir section 13) |

Clés `Guid` v7 : triables par ordre de création, ce qui évite un index supplémentaire
sur les dates, et non/devinables à partir d'un compteur.

Suppression logique appliquée au contenu publié (`Article`, `Resource`, `CommunityEvent`,
`Project`, `Comment`, `Partner`). Les enregistrements d'inscription, de signalement et les
comptes ne sont pas supprimés logiquement : un compte se désactive.

Le domaine ne référence l'utilisateur que par son `Guid`. `ApplicationUser` habite
l'infrastructure, car ASP.NET Identity est un détail technique ; le domaine ne doit pas
en dépendre. Les requêtes qui ont besoin du nom de l'auteur font une jointure explicite.

---

## 6. Arborescence du site

Dix pages, plus un pied de page commun à toutes.

| # | Page | Contenu principal |
|---|---|---|
| 1 | Accueil | Bannière avec logo et « La communauté des développeurs .NET au Niger », événements à venir, ressources populaires, présentation de l'objectif, accès aux réseaux |
| 2 | À propos | Mission, histoire de la communauté, équipe |
| 3 | Ressources | Formations et tutoriels, bibliothèques et outils, catégories par technologie |
| 4 | Communauté | Meetups et événements, projets open source locaux, contribution |
| 5 | Blog | Nouveautés de .NET, mises à jour des projets locaux, interviews de développeurs |
| 6 | Événements et Meetups | Calendrier, formulaire d'inscription, événements passés avec vidéos ou résumés |
| 7 | Partenaires et Sponsors | Partenaires locaux, sponsors d'événements et de ressources |
| 8 | Contact | Formulaire pour questions, suggestions et propositions de collaboration, réseaux sociaux |
| 9 | Mentions légales | *nouvelle* — exigée par le pied de page |
| 10 | Politique de confidentialité | *nouvelle* — exigée par le pied de page |

La page 4 « Communauté » figurait dans la spécification mais avait été omise de la
liste initiale. Elle est rétablie.

---

## 7. Rôles et permissions

Deux rôles uniquement. L'appartenance à l'équipe est un **booléen**, pas un rôle.

| Action | Membre | Admin |
|---|:---:|:---:|
| S'inscrire, valider son email | ✅ | ✅ |
| Modifier son profil | ✅ | ✅ |
| S'inscrire à un événement | ✅ | ✅ |
| Publier une ressource | ✅ | ✅ |
| Commenter un événement, un article, un billet | ✅ | ✅ |
| Signaler un contenu | ✅ | ✅ |
| Créer un article ou un billet de blog | ❌ | ✅ |
| Créer un événement | ❌ | ✅ |
| Créer un projet | ❌ | ✅ |
| Masquer ou supprimer un commentaire, une ressource | ❌ | ✅ |
| Traiter les signalements | ❌ | ✅ |
| Gérer les membres, l'équipe, les administrateurs | ❌ | ✅ |
| Envoyer la newsletter | ❌ | ✅ |

### Inscription

Ouverte à tous, avec validation par email. Le lien de validation expire.

---

## 8. Espace membre

Trois sections, cohérentes avec le tableau ci-dessus :

1. **Mon profil** — photo, nom, ville, biographie, liens GitHub / LinkedIn / X,
   compétences et technologies, numéro de téléphone optionnel et public,
   date d'adhésion.
2. **Mes inscriptions aux événements**
3. **Mes ressources publiées**

L'exploration du contenu n'est pas dupliquée dans l'espace membre : les pages publiques
suffisent.

---

## 9. Contenu et modération

- **Ressources** : publiées immédiatement par les membres, sans validation préalable.
- **Commentaires** : publiés immédiatement, **réservés aux membres connectés**.
- **Modération a posteriori** : bouton « Signaler » sur les commentaires et les
  ressources, les signalements arrivent dans le tableau de bord admin, qui peut masquer
  ou supprimer.
- **Pas de blocage de compte**, pas de note privée sur les ressources. Ces outils ont
  été écartés au profit du seul signalement.

---

## 9 bis. API HTTP

**Style d'écriture : contrôleurs MVC**, et non minimal API. Les contrôles, les filtres
d'autorisation, la validation automatique des modèles et la génération du document
OpenAPI s'écrivent de la même façon avec `[ApiController]`, ce qui évite de tenir deux
styles d'écriture dans un même projet. `AddControllers()` et `MapControllers()` sont
utilisés, et les contrôleurs vivent dans le projet Web.

### Arborescence

Les fichiers sont rangés par rôle, comme le reste de la solution :

```
src/DotnetNiger.Web/Controller/
├── AdminAccountsController.cs  gestion des comptes et des rôles, réservée à l'admin
├── ArticlesController.cs      contrôleur des articles du blog
├── CommentsController.cs      commentaires et modération
├── EventsController.cs        événements, inscriptions et annulations
├── MembersController.cs       profils publics et profil du membre connecté
├── PartnersController.cs      partenaires et sponsors
├── ProjectsController.cs      projets open source
├── ReportsController.cs       signalements et modération
├── ResourcesController.cs     ressources et formations
├── Common/
│   ├── ApiControllerBase.cs      socle : pagination, auteurs, autorisation, débit
│   ├── ApiLog.cs                 journalisation source-générée de l'API
│   └── DomainExceptionHandler.cs règles métier traduites en ProblemDetails
└── Contracts/
    └── ApiContracts.cs       requêtes, réponses, projections et ApiRoutes
```

Le nommage des segments d'URL est en anglais, cohérent avec les noms de tables et de
contrôleurs. Aucun préfixe de version, voir section 15.

### Écriture : qui peut faire quoi

L'écriture suit la propriété du contenu plutôt qu'un rôle unique. Tout membre peut
proposer un article ou une ressource ; seul l'auteur ou un administrateur peut ensuite le
modifier, l'archiver ou le retirer. Les événements et les partenaires engagent
l'association : leur création et leur modification sont réservées à l'administration. Un
commentaire peut être retiré par son auteur, et masqué, republié ou retiré par
l'administration. Le profil se modifie par son porteur, sur `/api/members/me`.

Les retraits sont **logiques** partout où c'est possible : un article archivé, un
commentaire masqué ou un partenaire désactivé disparaissent de l'affichage mais restent en
base, parce que d'autres lignes les référencent. Un compte n'est jamais effacé pour la même
raison — ses articles et ses commentaires portent son identité ; sa suppression le désactive
et lui retire ses rôles.

### Comptes et rôles

`/api/admin/accounts` est la seule surface d'administration des comptes. Elle expose la
liste et le détail, l'activation et l'appartenance à l'équipe, le remplacement ou l'ajout et
le retrait de rôles, le déverrouillage d'un compte bloqué, la validation d'une adresse, et la
désactivation. Les règles ajoutées ne sont pas décoratives :

- un administrateur ne peut ni se désactiver, ni retirer son propre rôle d'administrateur,
  ni se supprimer : cela le verrouillerait hors de l'application ;
- le dernier rôle d'administrateur **actif** ne peut pas être retiré, sous peine de laisser
  le site sans personne pour l'administrer ;
- la désactivation retire les rôles *après* l'enregistrement de l'état, pour qu'un jeton
  encore valide ne reste pas administrateur.

Les rôles sont résolus par rôle et non par compte : l'application en compte quelques
dizaines, alors qu'une page d'administration peut en lister davantage.

### Images de couverture

Quatre entités portent une image, utilisée par les vignettes des listes et des pages de
détail : `Article.CoverImageUrl`, `Project.ImageUrl`, `Resource.CoverImageUrl` et
`CommunityEvent.CoverImageUrl`. Les deux dernières colonnes ont été ajoutées par la
migration `AddCoverImages`. Les commentaires n'ont pas d'image, et les partenaires
conservent leur `LogoUrl` propre, qui ne fait pas partie des vignettes de contenu.

### Authentification des appels

L'API réutilise le cookie Identity du site. Une requête anonyme vers une route protégée
reçoit `401`, jamais une redirection vers une page de connexion, la redirection étant
réservée aux pages. Les règles de sécurité appliquées au site s'appliquent aussi aux
appels : mot de passe obligatoire pour tout compte créé depuis un fournisseur externe,
jeton externe à usage unique expirant en 15 minutes, et annulation de la liaison si le
rattachement échoue.

---

## 10. Design system

Palette confirmée, **trois couleurs chromatiques maximum** sur base blanche :

| Rôle | Couleur | Valeur | Emploi |
|---|---|---|---|
| Primaire | Violet .NET | `#512BD4` | Couleur d'identité, dominante |
| Accent | Vert Niger | `#008751` | Petites doses : succès, agenda en cours |
| Accent | Orange Niger | `#E3660B` | Petites doses : alerte, action d'événement |
| Fond | Blanc | `#FFFFFF` | Couleur dominante du site |

Exception fonctionnelle : un rouge réservé à l'erreur et à l'annulation.

Couleurs de la maquette A interdites : `#F04E23`, `#0039A6`, `#FFCC00`.

Règles retenues :

- La couleur est un **identifiant**, jamais une décoration. Une teinte ne sert que si elle
  porte un sens.
- Tout espacement est un **multiple de 8**. La gouttière est 24 px.
- **Sept tailles** de texte, **quatre graisses**, une ou deux par élément.
- **Une seule grille** de 12 colonnes.
- Le **rouge** n'existe que pour « erreur » et « annulé ».
- Un seul bouton principal par zone de décision.
- Texte jamais justifié, toujours aligné à gauche.

Polices : Inter.

### Mise en œuvre

Tailwind CSS 4, sans fichier de configuration : le thème est écrit directement en CSS
avec `@theme` dans `src/DotnetNiger.Web/wwwroot/css/design/theme.css`. Les jetons de
couleur y sont déclarés une fois et réutilisés comme utilitaires.

- Source : `wwwroot/css/design/theme.css`
- Sortie : `wwwroot/css/app.css`, **non versionnée**
- Compilation : cible MSBuild dans `DotnetNiger.Web.csproj`, exécutée avant chaque build
- Itération locale : `npm run css:watch`
- Node et npm sont requis pour compiler le projet. Pour compiler sans Node :
  `dotnet build -p:TailwindEnabled=false`

Aucune classe CSS n'est écrite à la main en dehors du fichier de thème.

---

## 11. Éditeur de contenu

**TinyMCE**, auto-hébergé dans `wwwroot`, licence GPL-2.0.

Barre d'outils complète avec contrôle de l'espacement, listes, liens, blocs de code pour
le C#, téléversement d'images, et un mode **Aperçu** qui rend la page telle qu'elle sera
publiée.

Auto-héberger plutôt que charger depuis un CDN, pour ne pas dépendre d'un tiers et pour
que l'éditeur fonctionne sans connexion.

Les images téléversées dans l'éditeur suivent la même règle que le reste : elles vont en
base de données.

---

## 12. Projets et contributeurs

- Les **projets sont créés par les admins uniquement**.
- Les **contributeurs sont récupérés automatiquement** depuis l'API GitHub.
- Chaque membre porte un champ « identifiant GitHub » qui permet le rapprochement
  automatique avec les contributeurs réels.

### Stratégie de récupération

**Hybride, avec repli sur la base** :

1. Le serveur interroge l'API GitHub.
2. Le résultat est mis en cache 6 heures et persisté en base.
3. La page ne dépend **jamais** de GitHub au moment de l'affichage.
4. Si l'API est indisponible ou atteint sa limite de débit, la dernière version connue
   est affichée.

Ce choix est imposé par l'hébergement : sur du mutualisé, on ne peut pas compter sur une
tâche de fond, et la limite anonyme de l'API GitHub, 60 requêtes par heure et par adresse
IP, est trop fragile depuis une adresse IP partagée. Un jeton d'API sera donc nécessaire,
stocké côté serveur uniquement, jamais dans le navigateur.

L'organisation GitHub sera créée plus tard. Le code est écrit en prévision, sans jeton.

---

## 13. Courriel et newsletter

### Envoi

Configuration entièrement paramétrable — hôte, port, utilisateur, mot de passe,
expéditeur — de sorte que le fournisseur puisse être changé sans toucher au code.

Pour les tests : SMTP Google avec mot de passe d'application. Cela suppose la double
authentification activée sur le compte.

**Réserve connue** : un compte Gmail grand public est plafonné autour de 500 envois par
jour et ses messages d'inscription sont fréquemment pris pour spam. Acceptable pour les
tests, pas durable. Le code est écrit pour permettre le changement de fournisseur.

Modèles de courriels personnalisés, stockés en base et modifiables par les admins.

### Newsletter

Diffusion réelle vers les abonnés lorsqu'un événement, un billet de blog ou une ressource
est créé.

Fonctionnement :

- Table d'abonnés, avec double validation à l'inscription puis désinscription en un clic.
- Table d'envoi qui enregistre les messages à diffuser.
- Envoi par lots, déclenché par une tâche planifiée du panneau de contrôle de l'hébergeur
  et pilotable à la main depuis le tableau de bord.

---

## 14. Langue

**Français uniquement.** La spécification ne demande pas de version Anglaise.

Pas d'infrastructure d'internationalisation, pas de version `/en/`, pas de traduction
automatique, pas de glossaire. Le site est déclaré en `lang="fr"` et le contenu est
rédigé directement en français.

---

## 15. Sécurité

- ASP.NET Core Identity, avec les rôles `Admin` et `Member`. Le mot de passe n'est jamais
  stocké en clair, et le cookie d'authentification est protégé contre le vol par jeton
  antifalsification, sans jeton de session à transporter.
- Secrets dans les secrets GitHub et dans la configuration du serveur, jamais dans le code
  et jamais dans le navigateur.
- En-têtes de sécurité, limitation de débit, protections contre la falsification de
  requêtes.
- Échappement systématique des contenus, en particulier le HTML produit par l'éditeur.
- Validation des entrées sur toutes les requêtes.
- Validation des inscriptions par email.

Si une API mobile ou un client tiers consommait un jour l'API, le jeton signé serait
asymétrique, jamais symétrique, et les clés seraient des certificats stockés hors du
dépôt. Ce n'est pas le cas aujourd'hui : l'API n'est consommée que par le site.

L'API n'est **pas versionnée dans l'URL** : les routes sont `/api/articles`,
`/api/resources`, `/api/events`, etc. Le nommage anglais des segments a été choisi
volontairement : `/api/v1` était envisagé au départ, puis écarté au profit de routes plus
courtes, l'API n'ayant qu'un seul consommateur, le site lui-même. Le document OpenAPI reste
servi à l'URL `/openapi/v1.json` : le « v1 » y désigne le format du document, pas la
version des endpoints.

L'API s'authentifie par le cookie Identity, comme le site : un appel protégé répond `401`
et non `403` lorsqu'il est anonyme, au lieu d'être redirigé vers une page de connexion.
Les appels JSON ne portent pas de jeton antifalsification ; la protection repose sur le
cookie `SameSite=Lax` et sur l'absence de cookie sur les requêtes inter-sites.

---

## 16. Points en attente

| Sujet | Manque |
|---|---|
| Identifiants OAuth | Secrets Google / Microsoft / GitHub non fournis |
| SMTP | Hôte, port, utilisateur, mot de passe et expéditeur non fournis |
| Nom de domaine | Aucun domaine fourni |
| Organisation GitHub | À créer, jeton d'API non encore disponible |
| Version de .NET à activer | À régler dans le panneau MonsterASP |
| Photos des membres | Fichiers disponibles dans `images_test/`, correspondance avec les membres non définie |
| Textes légaux | Textes génériques à faire relire par le porteur du projet |
| Mot de passe administrateur | Procédure de création à définir |
| Adresse d'expéditeur | À définir pour le SMTP |
| Vignettes d'images dans les pages | Les colonnes et l'API portent les images, l'affichage Blazor reste à faire |
