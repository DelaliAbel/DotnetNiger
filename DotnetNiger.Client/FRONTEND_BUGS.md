# Bugs Frontend — Updates à corriger

Ce fichier documente les bugs identifiés dans les formulaires d'édition côté frontend (Blazor WASM). **Le backend a été corrigé** pour ne plus effacer les tags/catégories si les champs sont `null`, mais **le frontend doit les peupler et les envoyer** pour que la mise à jour fonctionne.

---

## 🔴 Critique — Perte de données à l'update

### Event (`EventEdit.razor` + `MyEvents/EventEdit.razor`)

| # | Problème | Fichier(s) | Priorité |
|---|----------|------------|----------|
| 1 | **`TagNames` jamais peuplé** → tags effacés à chaque update. Le backend remplace tous les tags par la liste reçue (vide). | `EventEdit.razor` — méthode `MapToRequest()` ou `HandleSubmit` | 🔴 |
| 2 | **`OrganizerName` jamais envoyé** — champ existe dans le formulaire HTML mais variable `organizerName` non assignée à `eventRequest` avant envoi. | `EventEdit.razor` — lignes ~114-117, ~388 | 🟡 |
| 3 | **`IsPublished` toujours `true`** — impossible d'enregistrer un brouillon. Ajouter bouton/bascule pour `IsPublished = false`. | `EventEdit.razor` — ligne ~551 | 🟢 |
| 4 | **Orphelins d'images** — images uploadées avant soumission non nettoyées si l'update échoue. | `EventEdit.razor` — upload via `ImageUploader` | 🟢 |

---

### Blog (`BlogEdit.razor` + `MyBlog/BlogEdit.razor`)

| # | Problème | Fichier(s) | Priorité |
|---|----------|------------|----------|
| 1 | **`SeoDescription` jamais peuplé** — DTO `UpdatePostRequest` a un champ `SeoDescription` mais formulaire ne le renseigne pas. | `BlogEdit.razor` — méthode `MapToUpdateRequest()` | 🟡 |
| 2 | **`CategoryIds` non éditable** — catégories pré-remplies depuis l'existant mais aucun sélecteur de catégories dans le formulaire. | `BlogEdit.razor` — UI | 🟡 |
| 3 | **`IsPublished` toujours `true`** — idem Event. `SaveDraft()` existe mais n'est relié à aucun bouton. | `BlogEdit.razor` — lignes ~260, ~268 | 🟢 |
| 4 | **Position tags incohérente** — Tags en haut vs en bas selon Admin/MyBlog. | `BlogEdit.razor` vs `MyBlog/BlogEdit.razor` | 🟢 |

---

### Resource (`RessourceEdit.razor`)

| # | Problème | Fichier(s) | Priorité |
|---|----------|------------|----------|
| 1 | **`CategoryIds` jamais peuplé** → catégories effacées à chaque update. Backend supprime toutes puis réinsère liste reçue (vide). | `RessourceEdit.razor` — lignes ~118-125 | 🔴 |
| 2 | **Tags non modifiables** — pas d'UI pour les tags dans le formulaire d'édition. Vérifier s'ils sont pré-remplis. | `RessourceEdit.razor` | 🟡 |

---

## 🟡 Transversal (tous types)

| # | Problème | Priorité |
|---|----------|----------|
| 1 | **Pas de protection contre perte de données** si requête PUT échoue (réseau, serveur). Aucun rollback côté frontend. | 🟢 |
| 2 | **Mock services incomplets** — `Mock/EventService.cs`, `Mock/PostService.cs`, `Mock/ResourceService.cs` n'implémentent pas correctement les updates (tags, catégories ignorés). | 🟡 |

---

## ✅ Ce qui est corrigé côté Backend

Le backend **ignore maintenant** `TagNames`, `TagIds`, `CategoryIds` si ils sont `null` (ne les efface plus) :

- `EventCommandService.UpdateAsync` : sync tags seulement si `request.TagNames != null`
- `PostCommandService.UpdateAsync` : sync tags si `request.TagNames != null`, sync categories si `request.CategoryIds != null`
- `ResourceCommandService.UpdateAsync` : sync tags si `request.TagNames != null`, sync categories si `request.CategoryIds != null`

**Mais** : pour que l'update fonctionne, le frontend **doit envoyer** ces champs (même liste vide = vider les associations).

---

## 📋 Checklist de validation par formulaire

### EventEdit
- [ ] `TagNames` : multi-select / chips pré-rempli depuis l'existant
- [ ] `OrganizerName` : bindé sur l'input existant
- [ ] `IsPublished` : toggle "Publié" / bouton "Enregistrer comme brouillon"
- [ ] Nettoyage images orphelines si erreur

### BlogEdit
- [ ] `CategoryIds` : multi-select catégories pré-rempli
- [ ] `SeoDescription` : textarea ajoutée + bindée
- [ ] `IsPublished` : toggle/bouton brouillon
- [ ] Position tags uniformisée (Admin vs MyBlog)

### RessourceEdit
- [ ] `CategoryIds` : multi-select catégories pré-rempli
- [ ] `TagNames` : UI tags (chips / multi-select) ajoutée

### Mock Services
- [ ] `Mock/EventService.UpdateEventAsync` : sync tags
- [ ] `Mock/PostService.UpdatePostAsync` : sync tags + categories
- [ ] `Mock/ResourceService.UpdateResourceAsync` : sync tags + categories

---

## 🔗 Liens utiles

- **Specs frontend** : `SPECS_FRONTEND.md` (tâches UI complètes)
- **Tâches frontend** : `FRONTEND_TASKS.md` (liste pour dev frontend)
- **Backend changes** : `CHANGELOG.md` [Unreleased] → Added Tag/Category sync