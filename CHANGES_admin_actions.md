# Modifications : Intégrer à l'équipe + Changer le rôle

## Nouveaux fichiers
- `Models/Requests/ChangeRoleRequest.cs` — DTO avec `UserId` (Guid) et `NewRole` (string)

## Fichiers modifiés

### Components/Admin/Shared/AdminActionDropdown.razor
- Ajout du paramètre `OnChangeRole` (`EventCallback<ChangeRoleRequest>`)
- Ajout du paramètre `OnAddToTeam` (`EventCallback<Guid>`)
- Nouvelle sous-vue "Changer le rôle" : clic → affiche 4 boutons (Admin, Membre, Modérateur, SuperAdmin) → `confirm()` JS → invoque `OnChangeRole`
- Nouvelle option "Intégrer à l'équipe" → invoque directement `OnAddToTeam`
- Barre de retour (flèche ←) dans la sous-vue rôle
- Injection de `IJSRuntime` pour la boîte de confirmation

### Components/Admin/Cards/UsersTable.razor
- Ajout des paramètres `OnAddToTeam` et `OnChangeRole`
- Passage des deux callbacks au `AdminActionDropdown`

### Components/Admin/Cards/TeamTable.razor
- Ajout du paramètre `OnChangeRole`
- Passage du callback au `AdminActionDropdown`

### Components/Admin/Cards/AdminUsersCard.razor
- Ajout des paramètres `OnAddToTeam` et `OnChangeRole`
- Passage des deux callbacks au `AdminActionDropdown`

### Components/Admin/Cards/AdminTeamCard.razor
- Ajout du paramètre `OnChangeRole`
- Passage du callback au `AdminActionDropdown`

### Pages/Admin/Users.razor
- Injection de `INotificationService`
- Nouveau handler `HandleAddToTeam(Guid userId)`
  - `IsTeamMember = true`, `Position = string.Empty`
  - `UserService.UpdateUserAsync(user)`
  - `NotificationService.SendNotificationAsync(userId, "...")`
  - Toast + rechargement
- Nouveau handler `HandleChangeRole(ChangeRoleRequest request)`
  - Remplace `user.Roles` par `[request.NewRole]`
  - `UserService.UpdateUserAsync(user)`
  - `NotificationService.SendNotificationAsync(userId, "...")`
  - Toast + rechargement
- Passage des callbacks à `UsersTable` et `AdminUsersCard`

### Pages/Admin/Team/Team.razor
- Injection de `INotificationService`
- Nouveau handler `HandleChangeRole(ChangeRoleRequest request)` (identique à Users.razor)
- Passage de `OnChangeRole` à `TeamTable` et `AdminTeamCard`

## Comportement
- **Intégrer à l'équipe** : action directe → `IsTeamMember=true`, `Position=""` → notification automatique
- **Changer le rôle** : sous-menu → sélection du rôle → confirmation `confirm()` → application + notification

## Endpoints API côté backend (manquants)

⚠️ **Fonctionne uniquement en mode mock** (`UseMockServices: true`). En mode API réel, les endpoints backend existants sont insuffisants.

### Changer le rôle — partiellement existant
- `POST api/identity/admin/users/{userId}/roles` existe mais **ajoute** seulement un rôle, ne remplace pas les rôles existants.
- `ApiUserService.UpdateUserAsync()` parcourt les nouveaux rôles et appelle `POST .../roles` pour ceux qui ne sont pas déjà présents. Les anciens rôles ne sont jamais supprimés.
- **Problème** : le frontend fait `user.Roles = new List<string> { newRole }`, mais l'API ne fait qu'ajouter, sans retirer les anciens rôles.

### Intégrer à l'équipe (IsTeamMember / Position) — inexistant
- `ApiUserService.UpdateUserAsync()` n'envoie que :
  - `PATCH .../status` → `{ isActive: bool }`
  - `POST .../roles` → `{ roleName: string }` (ajout seulement)
- `IsTeamMember` et `Position` ne sont **jamais envoyés** dans aucune requête HTTP.
- La modification est silencieusement ignorée en mode API.

### Ce qu'il faudrait côté backend
- Nouvel endpoint `PUT api/identity/admin/users/{userId}` acceptant un `UserDto` complet (ou au moins `IsTeamMember`, `Position`, `Roles`, `Skills`)
- Ou des endpoints dédiés :
  - `PATCH .../team` → `{ isTeamMember, position }`
  - `PUT .../roles` → `{ roles: [...] }` (remplacement complet, pas un simple ajout)

---

## Skills / Technologies — fonction d'édition manquante côté profil

### Constat
Les compétences (`Skills`) sont présentes dans `UserDto` (`List<string>`) et affichées sur les pages de visualisation (`/profile`, `/admin/profile/{id}`), mais il est **impossible de les modifier**.

### Ce qui existe
- `UserDto.Skills` — propriété `List<string>` présente
- Pages de visualisation : affichent les skills sous forme de badges
- `MockUserService.UpdateUserAsync()` — copie les skills (mode mock seulement)

### Ce qui manque

#### 1. `UpdateProfileRequest.cs`
Le DTO de mise à jour du profil n'a **pas** de propriété `Skills` :
```
public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    // ❌ Skills manquant
}
```

#### 2. Pages d'édition profil
- `Pages/Profile/EditProfile.razor` (`/profile/informations`)
- `Pages/Admin/Profile/EditProfile.razor` (`/admin/profile/informations`)
- `SyncFormFromState()` ne mappe pas les skills
- Aucun champ de saisie pour ajouter/supprimer des compétences

#### 3. API
- `ApiProfileService.UpdateProfileAsync()` envoie `UpdateProfileRequest` via `PUT api/me`
- Aucun endpoint dédié pour les skills (`api/me/skills` ou autre)
- `ApiEndpoints.cs` ne définit pas de route pour les skills

### Ce qu'il faudrait
- Ajouter `List<string>? Skills` à `UpdateProfileRequest`
- Ajouter un champ de saisie (tag input / textarea) dans les pages d'édition profil
- Optionnel : créer un endpoint `PUT api/me/skills` dédié
- Coté mock : `ProfileService.UpdateProfileAsync()` devrait aussi mapper les skills

---

## Correctifs appliqués

### Dashboard — KPIs utilisateurs corrompu

**Fichier :** `Pages/Admin/Dashboard.razor`

**Problème :** La KPI "Utilisateurs" affichait `1` (profil courant) au lieu du nombre total d'utilisateurs.

```csharp
// AVANT (bug)
var users = await UserService.GetPendingUsersAsync();
// ...
new() { IconClass = "fas fa-users", Value = CurrentProfile is null ? 0 : 1, ... }

// APRÈS
var allUsers = await UserService.GetUsersAsync();
// ...
new() { IconClass = "fas fa-users", Value = allUsers.Count, ... }
```

---

### Blog — `PublishedAt` nullable comparé à `DateTime.MinValue`

**Fichiers modifiés :**
- `Pages/Admin/Blog/Blog.razor` (filtre + dropdown)
- `Components/Admin/Cards/BlogTable.razor`
- `Components/Admin/Cards/AdminBlogCard.razor`
- `Pages/Admin/Blog/Details.razor`

**Problème :** `PostDto.PublishedAt` est `DateTime?` (nullable). Les comparaisons `p.PublishedAt != DateTime.MinValue` renvoient `true` quand la valeur est `null`, ce qui traite incorrectement les brouillons comme publiés.

**Correctif :** Remplacer par `p.PublishedAt.HasValue`.

**Ajout :** Option "Brouillon" dans le `<select>` du filtre de statut sur `Blog.razor`.

---

### Blog — Erreurs de désérialisation JSON avalées silencieusement

**Fichier :** `Services/Api/ApiResponseReader.cs`

**Problème :** Le `catch` vide dans `Deserialize<T>()` ne laissait aucune trace des échecs JSON.

**Correctif :** Log vers `Console.Error` avec le type attendu et les 500 premiers caractères du JSON.

---

### Upload — `StreamContent` incompatible avec Blazor WebAssembly

**Fichier :** `Services/Api/ApiUploadService.cs`

**Problème :** `StreamContent` ne se sérialise pas correctement via `BrowserHttpMessageHandler` (fetch API) en Blazor WASM.

**Correctif :** Lire le fichier en `byte[]` d'abord, utiliser `ByteArrayContent` à la place de `StreamContent`.

### Upload — Corps des réponses d'erreur perdu

**Fichier :** `Services/Api/ApiUploadService.cs`

**Problème :** Les messages d'erreur détaillés du serveur n'étaient pas lus (seul le status code était retourné).

**Correctif :** Lecture du body via `ReadErrorBodyAsync()` et inclusion dans le message d'erreur.

### Upload — Aucun log

**Fichier :** `Services/Api/ApiUploadService.cs`

**Problème :** Aucune trace des échecs d'upload.

**Correctif :** Injection de `ILogger<ApiUploadService>`, logs sur les uploads réussis et échoués.

**Fichier :** `Program.cs`

**Correctif :** Passage du logger dans l'enregistrement DI de `ApiUploadService`.
