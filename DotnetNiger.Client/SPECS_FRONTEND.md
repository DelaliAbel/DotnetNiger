# Spécifications Frontend — À implémenter par le dev frontend

Ce fichier liste les tâches UI/UX confiées au développeur frontend.
Chaque spécification décrit le comportement attendu, les maquettes conceptuelles, et les contraintes techniques.

---

## 📊 Statut global

| Spécification | Status | Backend prêt ? |
|--------------|--------|----------------|
| 1. Dashboard Collaborator | 🔴 À faire | ✅ Oui (`GET /admin/stats/mine`) |
| 2. Composant "Permission refusée" | 🔴 À faire | ⚠️ Partiel (endpoints roles/permissions à créer) |
| 3. Page Admin Permissions par rôle | 🔴 À faire | ❌ Non (endpoints manquants) |
| 4. Onglets Settings (Features/Registration) | 🔴 À faire | ⚠️ À vérifier |
| 5. Page listing Projets (`/projets`) | 🔴 À faire | ✅ Oui (`GET /projects`) |
| 6. Corrections formulaires édition | 🔴 À faire | ✅ Oui (backend ignore null) |

---

## 1. Dashboard Collaborator (stats personnelles)

**Contexte** : Actuellement le Dashboard (`/admin/dashboard`) redirige tout utilisateur qui n'est pas Admin/SuperAdmin vers `/admin/profile`. On veut que les **Collaborator** (certificat validé) aient aussi un tableau de bord, adapté à leur scope.

### Comportement attendu

| Rôle | Comportement |
|------|--------------|
| **Admin** / **SuperAdmin** | Dashboard actuel (stats globales : total events, blogs, ressources, membres, etc.) |
| **Collaborator** (certificat validé) | Dashboard **stats personnelles** : mes events, mes blogs, mes ressources, mes projets + 5 derniers éléments créés |
| **User** simple | Redirection vers `/admin/profile` (inchangé) |

### Endpoint backend

```
GET /api/v1/admin/stats/mine
Authorization: Bearer <token>
Roles requis: Collaborator, Admin, SuperAdmin
```

**Réponse** :
```json
{
  "eventsCount": 5,
  "blogsCount": 3,
  "resourcesCount": 2,
  "projectsCount": 1
}
```

### UI attendue

- Même layout que le Dashboard Admin (`AdminLayout.razor`)
- Grille de 4 cartes de stats (même style : icône, chiffre, label)
- Section "Derniers éléments" : liste des 5 derniers créés (type, titre, date, lien vers édition)
- Accès conditionnel dans `Dashboard.razor` : vérifier `AuthService.CurrentUser.Roles` ou `PermissionService.HasPermission("Dashboard.ViewMine")`

### Contrainte technique

- Utiliser le layout `AdminLayout.razor` (déjà en place)
- Cartes de stats : même composant/style que Dashboard Admin existant
- Ajouter une section conditionnelle dans `Dashboard.razor` (pas une nouvelle page)

---

## 2. Composant "Permission refusée" réutilisable

**Contexte** : Quand un user clique sur un lien ou tente une action sans la permission requise, on veut un feedback visuel clair au lieu d'un 403 silencieux ou page blanche.

### Comportement attendu

- Petit popup/toast/alert en haut de page : "Vous n'avez pas la permission nécessaire pour effectuer cette action."
- Composant réutilisable : `<PermissionDenied Message="..." />`
- Optionnel : bouton "Retour" ou "Contacter l'administrateur"
- Auto-disparition après 5 secondes (optionnel)

### Où l'utiliser

- Dans chaque page qui fait des appels API protégés par permission
- Dans la Sidebar si un lien est cliqué mais user n'a pas le rôle requis (bien que la Sidebar cache déjà les liens, sécurité supplémentaire)
- Dans `RedirectToLogin.razor` si user connecté mais sans le rôle

### Maquette conceptuelle

```
┌──────────────────────────────────────────────────┐
│ ⚠️ Accès refusé                                  │
│ Vous n'avez pas la permission "EventCreate".     │
│ ┌──────────┐  ┌──────────────┐                   │
│ │ Retour   │  │ Contacter    │                   │
│ └──────────┘  │ admin        │                   │
│               └──────────────┘                   │
└──────────────────────────────────────────────────┘
```

---

## 3. Page Admin Gestion des Permissions par Rôle

**Contexte** : Actuellement, les permissions sont assignées aux rôles via le code (`PermissionNames.cs`) et la base de données. Un SuperAdmin doit pouvoir gérer ça depuis l'interface.

### Emplacement

- Dans un onglet "Permissions" de la page Settings (`/admin/settings/permissions`)
- OU une page dédiée `/admin/permissions`
- Accessible uniquement aux **SuperAdmin**

### Comportement attendu

- Liste des rôles existants (SuperAdmin, Admin, Collaborator, User, Client)
- Pour chaque rôle, liste de **toutes les permissions possibles** avec checkbox
- Permissions groupées par catégorie (Events, Blogs, Projects, Resources, Admin)
- Bouton "Sauvegarder" qui appelle l'API de mise à jour

### Maquette conceptuelle

```
┌────────────────────────────────────────────┐
│ Gestion des permissions                    │
│ ─────────────────────────────────────────  │
│                                            │
│ Rôle : SuperAdmin                          │
│ ┌──────────────────────────────────────┐   │
│ │ ☑ Toutes les permissions             │   │
│ └──────────────────────────────────────┘   │
│                                            │
│ Rôle : Collaborator                        │
│ ┌─ Events ────────────────────────────┐    │
│ │ ☑ Voir les events                   │    │
│ │ ☑ Créer un event                    │    │
│ │ ☑ Modifier ses events               │    │
│ │ ☐ Supprimer ses events              │    │
│ │ ☐ Approuver les events              │    │
│ └─────────────────────────────────────┘    │
│ ┌─ Blogs ─────────────────────────────┐    │
│ │ ☑ Voir les blogs                    │    │
│ │ ☐ Créer un blog                     │    │
│ │ ☐ Modifier ses blogs                │    │
│ └─────────────────────────────────────┘    │
│ ┌─ Projets ───────────────────────────┐    │
│ │ ...                                  │    │
│ └─────────────────────────────────────┘    │
│                                            │
│ ┌──────────────────────┐                    │
│ │    Sauvegarder       │                    │
│ └──────────────────────┘                    │
└────────────────────────────────────────────┘
```

### API nécessaires (Backend à créer)

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/v1/admin/roles` | GET | Liste des rôles avec leurs permissions actuelles |
| `/api/v1/admin/permissions` | GET | Liste de toutes les permissions possibles (catégorisées) |
| `/api/v1/admin/roles/{roleId}/permissions` | PUT | Met à jour les permissions d'un rôle |

### Contrainte technique

- Utiliser `AuthorizeView` avec `Roles="SuperAdmin"` pour restreindre l'accès
- Les permissions sont définies dans `Helpers/PermissionNames.cs`
- Utiliser les services existants (`PermissionService`, `RoleService`) déjà mockés

---

## 4. Page Settings — Onglets "Fonctionnalités" & "Inscription"

**Contexte** : Dans `Pages/Admin/Settings/Settings.razor` (lignes 114-205), de larges sections pour les feature toggles et registration mode sont commentées avec `TODO : Partie a revoir`.

### À réactiver

| Section | Contrôles | Description |
|---------|-----------|-------------|
| **Fonctionnalités** | Toggle "Approbation événements requise" | Si ON : nouveaux events = PendingReview |
| | Toggle "Approbation blogs requise" | Si ON : nouveaux posts = PendingReview |
| | Toggle "Newsletter activée" | Affiche/masque formulaire inscription newsletter |
| | Toggle "Upload activé" | Affiche/masque composants d'upload images/fichiers |
| **Inscription** | Radio "Ouverte" / "Sur invitation" / "Fermée" | Contrôle qui peut créer un compte |

### Contrainte technique

- Endpoints backend : vérifier `ISettingsService` / `SettingsController` existants
- Sauvegarde via bouton unique "Enregistrer les paramètres"
- Afficher toast de confirmation

---

## 5. Page Listing Projets — `/projets` (Nouvelle page)

**Manquante** : Il existe `/projets/{slug}` (`Detail.razor`) mais pas de page de listing.

### À créer

- `Pages/Projects/List.razor` avec `@page "/projets"`
- Grid responsive (cards) : image, titre, description courte, technologies, statut
- Filtres : recherche textuelle, statut, technologies
- Pagination
- Appel `GET /api/v1/projects` (existe ✅)

---

## 6. Corrections formulaires édition (Tags, Catégories, Brouillons)

**Contexte** : Le backend ignore maintenant `TagNames`/`CategoryIds` si `null` (ne les efface plus). Mais le frontend **doit les envoyer** pour que l'update fonctionne.

### À corriger dans chaque formulaire

| Formulaire | Champs à binder | Action |
|------------|-----------------|--------|
| `EventEdit.razor` | `TagNames` (List<string>), `OrganizerName` (string), `IsPublished` (bool?) | Multi-select tags, input text, toggle/bouton "Brouillon" |
| `BlogEdit.razor` | `CategoryIds` (List<Guid>), `SeoDescription` (string), `IsPublished` (bool?) | Multi-select catégories, textarea, toggle/bouton "Brouillon" |
| `RessourceEdit.razor` | `CategoryIds` (List<Guid>), `TagNames` (List<string>) | Multi-select catégories, multi-select tags |

### Mock Services

Mettre à jour `Mock/EventService.cs`, `Mock/PostService.cs`, `Mock/ResourceService.cs` pour gérer correctement les updates (tags, catégories) en mode démo.

---

## Calendrier suggéré

| Priorité | Tâche | Dépendance |
|----------|-------|------------|
| P0 | Composant "Permission refusée" | Aucune |
| P1 | Dashboard Collaborator | Endpoint backend stats/mine ✅ |
| P2 | Page Permissions (SuperAdmin) | Endpoints backend roles/permissions ❌ |
| P2 | Onglets Settings | Endpoints backend settings ⚠️ |
| P2 | Page `/projets` listing | Endpoint backend projects ✅ |
| P3 | Corrections formulaires édition | Backend prêt ✅ |
| P3 | Footer / Home fixes | Aucune |
| P3 | Mock services | Aucune |

---

## ❓ Questions / Blocages

Si un endpoint manque ou retourne une erreur, ouvrir une issue GitHub avec le tag `backend-needed`.