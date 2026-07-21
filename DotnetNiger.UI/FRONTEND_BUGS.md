# Bugs Frontend — Updates à corriger

## Event (`EventEdit.razor` + `MyEvents/EventEdit.razor`)

| # | Problème | Fichier(s) | Priorité |
|---|----------|-----------|----------|
| 1 | **`TagNames` jamais peuplé** → les tags sont effacés à chaque update. Le backend remplace tous les tags par la liste reçue (vide). | `EventEdit.razor` — méthode `MapToRequest()` ou `HandleSubmit` | 🔴 |
| 2 | **`OrganizerName` jamais envoyé** — le champ existe dans le formulaire HTML mais la variable `organizerName` n'est pas assignée à `eventRequest` avant l'envoi. | `EventEdit.razor` — lignes ~114-117, ~388 | 🟡 |
| 3 | **`IsPublished` toujours `true`** — impossible d'enregistrer un brouillon depuis l'édition. Ajouter un bouton/bascule pour `IsPublished = false`. | `EventEdit.razor` — ligne ~551 | 🟢 |
| 4 | **Orphelins d'images** — les images uploadées avant soumission ne sont pas nettoyées si l'update échoue. | `EventEdit.razor` — upload via `ImageUploader` | 🟢 |

## Blog (`BlogEdit.razor` + `MyBlog/BlogEdit.razor`)

| # | Problème | Fichier(s) | Priorité |
|---|----------|-----------|----------|
| 1 | **`SeoDescription` jamais peuplé** — le DTO `UpdatePostRequest` a un champ `SeoDescription` mais le formulaire ne le renseigne pas. | `BlogEdit.razor` — méthode `MapToUpdateRequest()` | 🟡 |
| 2 | **`CategoryIds` non éditable** — les catégories sont pré-remplies depuis l'existant mais il n'y a aucun sélecteur de catégories dans le formulaire. | `BlogEdit.razor` — UI | 🟡 |
| 3 | **`IsPublished` toujours `true`** — idem Event. `SaveDraft()` existe mais n'est relié à aucun bouton. | `BlogEdit.razor` — ligne ~260, ~268 | 🟢 |
| 4 | **Position des tags incohérente** — Tags en haut vs en bas selon Admin/MyBlog. | `BlogEdit.razor` vs `MyBlog/BlogEdit.razor` | 🟢 |

## Resource (`RessourceEdit.razor`)

| # | Problème | Fichier(s) | Priorité |
|---|----------|-----------|----------|
| 1 | **`CategoryIds` jamais peuplé** → les catégories sont effacées à chaque update. Le backend supprime toutes les catégories puis réinsère celles reçues (liste vide). | `RessourceEdit.razor` — lignes ~118-125 | 🔴 |
| 2 | **Tags non modifiables** — pas d'UI pour les tags dans le formulaire d'édition. Vérifier s'ils sont pré-remplis. | `RessourceEdit.razor` | 🟡 |

## Cross-cutting (tous les types)

| # | Problème | Priorité |
|---|----------|----------|
| 1 | **Pas de protection contre la perte de données** si la requête PUT échoue (réseau, serveur). Aucun rollback côté frontend. | 🟢 |
| 2 | **Mock services incomplets** — `Mock/EventService.cs`, `Mock/PostService.cs`, `Mock/ResourceService.cs` n'implémentent pas correctement les updates (tags, catégories ignorés). | 🟡 |
