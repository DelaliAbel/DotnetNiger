# Spécifications frontend — À implémenter

Ce fichier liste les tâches UI confiées au développeur frontend.
Chaque spécification décrit le comportement attendu, les maquettes conceptuelles, et les contraintes techniques.

---

## 1. Dashboard Collaborator (stats personnelles)

**Contexte :** Actuellement le Dashboard (`/admin/dashboard`) redirige tout utilisateur qui n'est pas Admin/SuperAdmin vers `/admin/profile`. On veut que les Collaborator aient aussi un tableau de bord, adapté à leur scope.

**Comportement attendu :**

- Si l'utilisateur est **Admin** ou **SuperAdmin** → Dashboard actuel (stats globales : nombre total events, blogs, ressources, membres, etc.)
- Si l'utilisateur est **Collaborator** (et certificate validé) → Dashboard avec stats personnelles :
  - Nombre de mes events (créés par lui)
  - Nombre de mes blogs
  - Nombre de mes ressources
  - Nombre de mes projets
  - Liste des 5 derniers éléments créés
- Si l'utilisateur est **User** simple → redirection vers `/admin/profile` (inchangé)

**Où mettre les stats :**

- Les endpoints d'agrégation par créateur sont côté Community. Si un endpoint du type `GET /api/v1/admin/stats/mine` n'existe pas, le backend devra le créer.
- Format attendu de la réponse :

```json
{
  "eventsCount": 5,
  "blogsCount": 3,
  "resourcesCount": 2,
  "projectsCount": 1,
  "recentItems": [
    { "type": "event", "title": "...", "createdAt": "..." },
    ...
  ]
}
```

**Contrainte technique :**

- Utiliser le layout `AdminLayout.razor` (déjà en place)
- Les cartes de stats doivent suivre le même style que le Dashboard Admin existant (grille de cartes avec icônes)
- Ajouter un onglet ou une section conditionnelle dans `Dashboard.razor`

---

## 2. Composant "Permission refusée"

**Contexte :** Quand un utilisateur clique sur un lien ou tente une action pour laquelle il n'a pas la permission, on veut un feedback visuel clair au lieu d'un 403 silencieux ou d'une page blanche.

**Comportement attendu :**

- Petit popup/toast/alert en haut de la page : "Vous n'avez pas la permission nécessaire pour effectuer cette action."
- Le composant doit être réutilisable : `<PermissionDenied Message="..." />`
- Optionnel : bouton "Retour" ou "Contacter l'administrateur"
- Auto-disparition après 5 secondes (optionnel)

**Où l'utiliser :**

- Dans chaque page qui fait des appels API protégés par permission
- Dans la Sidebar si un lien est cliqué mais l'utilisateur n'a pas le rôle requis (bien que la Sidebar cache déjà les liens, on peut garder une sécurité supplémentaire)
- Dans `RedirectToLogin.razor` si l'utilisateur est connecté mais n'a pas le rôle

**Maquette conceptuelle :**

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

## 3. Page admin de gestion des permissions par rôle

**Contexte :** Actuellement, les permissions sont assignées aux rôles via le code (`PermissionNames.cs`) et la base de données. Un SuperAdmin doit pouvoir gérer ça depuis l'interface.

**Emplacement :**

- Dans un onglet "Permissions" de la page Settings (`/admin/settings/permissions`)
- OU une page dédiée `/admin/permissions`
- Accessible uniquement aux **SuperAdmin**

**Comportement attendu :**

- Liste des rôles existants (SuperAdmin, Admin, Collaborator, User)
- Pour chaque rôle, liste de toutes les permissions possibles avec une checkbox
- Les permissions sont regroupées par catégorie (Events, Blogs, Projects, Resources, Admin)
- Enregistrement via un bouton "Sauvegarder" qui appelle l'API

**Maquette conceptuelle :**

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
│ │ ☐ Supprimer ses blogs               │    │
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

**API nécessaire :**

- `GET /api/v1/admin/roles` — liste des rôles avec leurs permissions actuelles
- `GET /api/v1/admin/permissions` — liste de toutes les permissions possibles
- `PUT /api/v1/admin/roles/{roleId}/permissions` — met à jour les permissions d'un rôle

**Contrainte technique :**

- Utiliser `AuthorizeView` avec `Roles="SuperAdmin"` pour restreindre l'accès
- Les permissions sont définies dans `Helpers/PermissionNames.cs`
- Utiliser les services existants (`PermissionService`, `RoleService`) déjà mockés

---

## Calendrier suggéré

| Priorité | Tâche                          | Dépendance                          |
| --------- | ------------------------------- | ------------------------------------ |
| P0        | Composant "Permission refusée" | Aucune                               |
| P1        | Dashboard Collaborator          | Endpoint backend stats               |
| P2        | Page permissions                | Endpoints backend rôles/permissions |
