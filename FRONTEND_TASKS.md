# Tâches Frontend — Pour le développeur frontend

Ce fichier liste les tâches UI/UX à implémenter côté frontend (Blazor WASM). Les endpoints backend correspondants sont **déjà implémentés**.

---

## 🔴 Priorité Haute (P1)

### 1. Corriger les bugs de perte de données (tags/catégories) — `EventEdit.razor`, `BlogEdit.razor`, `RessourceEdit.razor`

**Problème** : Les formulaires d'édition n'envoient pas `TagNames`, `CategoryIds` ou `OrganizerName` → le backend les reçoit vides et **efface** les associations existantes.

**Fichiers concernés** :
- `Pages/Events/EventEdit.razor` + `Pages/MyEvents/EventEdit.razor`
- `Pages/Blog/BlogEdit.razor` + `Pages/MyBlog/BlogEdit.razor`
- `Pages/Resources/RessourceEdit.razor`

**Corrections à faire** :

| Formulaire | Champs manquants | Action |
|---|---|---|
| EventEdit | `TagNames` (List<string>) | Pré-remplir depuis l'existant + champ multi-select / chips |
| EventEdit | `OrganizerName` (string) | Lier le champ input à la propriété du model |
| BlogEdit | `CategoryIds` (List<Guid>) | Ajouter sélecteur catégories (multi-select) + pré-remplir |
| BlogEdit | `SeoDescription` (string) | Ajouter champ textarea + binder |
| BlogEdit | `IsPublished` (bool?) | Ajouter bouton "Enregistrer comme brouillon" → `IsPublished = false` |
| RessourceEdit | `CategoryIds` (List<Guid>) | Ajouter sélecteur catégories + pré-remplir |

**Endpoint backend corrigé** : Le backend ignore maintenant `TagNames`/`CategoryIds` **si null** (ne les efface plus). Mais il faut que le frontend **les envoie** pour que l'update fonctionne.

---

### 2. Dashboard Collaborateur — `Pages/Admin/Dashboard.razor`

**Contexte** : Actuellement redirige les non-Admins vers `/admin/profile`.

**Nouveau comportement** :
- **Admin / SuperAdmin** → Dashboard actuel (stats globales)
- **Collaborator** (certificat validé) → Dashboard **stats personnelles**
- **User** simple → Redirection `/admin/profile` (inchangé)

**Endpoint backend** : `GET /api/v1/admin/stats/mine` (✅ implémenté)
```json
{
  "eventsCount": 5,
  "blogsCount": 3,
  "resourcesCount": 2,
  "projectsCount": 1
}
```

**UI attendue** (même style que dashboard admin) :
- 4 cartes : Mes événements, Mes blogs, Mes ressources, Mes projets
- Liste des 5 derniers éléments créés (type, titre, date)
- Layout : `AdminLayout.razor` déjà en place

**Rôles** : Vérifier via `AuthService.CurrentUser.Roles` ou `PermissionService.HasPermission("Dashboard.ViewMine")`

---

### 3. Page listing Projets — `/projets` (nouvelle page)

**Manquante** : Il y a `/projets/{slug}` (Detail.razor) mais pas de listing.

**À créer** :
- `Pages/Projects/List.razor` avec `@page "/projets"`
- Grid responsive (cards) : image, titre, description courte, technologies, statut
- Filtres : recherche textuelle, statut, technologies
- Pagination
- Appel `GET /api/v1/projects` (existe ✅)

---

## 🟡 Priorité Moyenne (P2)

### 4. Composant `<PermissionDenied>` réutilisable

**Spéc** : `SPECS_FRONTEND.md` — §2

**Utilisation** :
```razor
<PermissionDenied Message="Vous n'avez pas la permission 'EventCreate'." />
```

**Affichage** : Toast / alert en haut de page, auto-dismiss 5s, bouton "Retour" optionnel.

**Où l'utiliser** :
- Dans chaque page protégée (wrapper dans `AdminLayout` ou par page)
- Dans `RedirectToLogin.razor` si user connecté mais sans rôle

---

### 5. Page Admin Permissions — `/admin/permissions` (SuperAdmin only)

**Spéc** : `SPECS_FRONTEND.md` — §3

**Fonctionnalités** :
- Liste des rôles (SuperAdmin, Admin, Collaborator, User)
- Pour chaque rôle : checklist des permissions groupées par catégorie (Events, Blogs, Projects, Resources, Admin)
- Bouton "Sauvegarder" → `PUT /api/v1/admin/roles/{roleId}/permissions` (à créer backend)

**API nécessaires** (backend à faire) :
- `GET /api/v1/admin/roles` → liste rôles + permissions actuelles
- `GET /api/v1/admin/permissions` → liste toutes permissions (catégorisées)
- `PUT /api/v1/admin/roles/{roleId}/permissions` → maj permissions d'un rôle

---

### 6. Onglets Settings : "Fonctionnalités" & "Inscription" — `Pages/Admin/Settings/Settings.razor`

**Actuellement** : Code commenté (lignes 114-205) avec `TODO : Partie a revoir`

**À réactiver** :
- Toggle "Approbation événements requise"
- Toggle "Approbation blogs requise"
- Toggle "Newsletter activée"
- Toggle "Upload activé"
- Mode inscription : Ouverte / Sur invitation / Fermée

**Endpoints backend** : Existent via `ISettingsService` / `SettingsController` (à vérifier)

---

## 🟢 Priorité Basse (P3)

### 7. Footer — `Components/Shared/Footer.razor`
- Téléphone : remplacer `+227 XXXXXXXX` par le vrai numéro
- Liens sociaux : remplacer `href="#"` par vraies URLs (Facebook, Twitter/X, LinkedIn, GitHub, Discord, WhatsApp)

### 8. Page Home — `Pages/Home.razor`
- Titre des 3 cartes "Why join" : corriger "Excelller" → titres distincts
- Icônes réseaux sociaux : inversées (Facebook affiché "Discord", WhatsApp affiché "X")

### 9. Nettoyage Mock Services
- `Mock/EventService.cs`, `Mock/PostService.cs`, `Mock/ResourceService.cs` : implémenter correctement les updates (tags, catégories) pour le mode démo

---

## 📋 Checklist de validation

- [ ] Tous les formulaires d'édition envoient `TagNames`, `CategoryIds`, `OrganizerName`, `SeoDescription`, `IsPublished`
- [ ] Dashboard collaborateur affiche stats personnelles + derniers items
- [ ] Page `/projets` liste les projets avec filtres/pagination
- [ ] Composant `PermissionDenied` fonctionne et utilisé
- [ ] Page permissions accessible aux SuperAdmin seulement
- [ ] Onglets Settings fonctionnels
- [ ] Footer et Home corrigés

---

## 🔗 Endpoints backend déjà prêts

| Fonctionnalité | Endpoint | Status |
|---|---|---|
| Stats collaborateur | `GET /api/v1/admin/stats/mine` | ✅ |
| CRUD Membres | `POST/PUT/DELETE /api/v1/members` | ✅ |
| Tags/Categories sync (update) | Backend ignore si null | ✅ |
| Projects listing | `GET /api/v1/projects` | ✅ |
| Project detail | `GET /api/v1/projects/slug/{slug}` | ✅ |

---

## ❓ Questions / Blocages

Si un endpoint manque ou retourne une erreur, ouvrir une issue GitHub avec le tag `backend-needed`.