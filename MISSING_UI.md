# UI manquantes — Pages et designs à implémenter

> Fichier listant uniquement les pages/designs UI restants.
> Tout le code (endpoints, services, DTOs, models) est implémenté côté backend/API.

---

## TODO — 1. Page admin Catégories

**Route :** `/admin/categories`
**Composant :** `Pages/Admin/Categories/Categories.razor`
**Layout :** `AdminLayout`

### Design

- **Header :** titre "Catégories" + bouton "Nouvelle catégorie" (ouvre un drawer/modal)

- **Tableau :**
  | Colonne | Type |
  |---------|------|
  | Nom | string (lien vers page publique filtrée) |
  | Slug | string |
  | Description | string (tronquée 100 car.) |
  | Nb articles | badge/compteur |
  | Actions | dropdown : Modifier, Supprimer |

- **Drawer création/édition :**
  - Champ `Name` (required), `Description` (textarea), `Slug` (généré auto, modifiable)
  - Boutons Annuler / Enregistrer

- Confirmation JS `confirm()` avant suppression

---

## TODO — 2. Page admin Tags

**Route :** `/admin/tags`
**Composant :** `Pages/Admin/Tags/Tags.razor`
**Layout :** `AdminLayout`

### Design

- Similaire catégories, sans description

- **Tableau :** Nom, Slug, Nb utilisations (badge), Actions (Modifier/Supprimer)
- **Modal :** Champ `Name` (required), `Slug` (auto)
- Confirmation JS avant suppression

---

## TODO — 3. Page admin Certificats

**Route :** `/admin/certificates`
**Composant :** `Pages/Admin/Certificates/Certificates.razor`
**Layout :** `AdminLayout`

### Design

- **Header :** "Validations des certificats"

- **Tabs :** `En attente` (default) | `Approuvés` | `Rejetés` (avec badge compteur)

- **Cartes (vue grille)** : chaque carte contient :
  - Avatar + Nom + Email du membre
  - Titre du certificat
  - Fichier/URL (lien, nouvel onglet)
  - Date de soumission
  - Statut (badge : orange Attente / vert Approuvé / rouge Rejeté)
  - **Actions si "En attente" :** Approuver (confirm JS) / Rejeter (modal avec champ `reason`)
  - Si rejeté : afficher le motif

- **Stats en haut :** X en attente | Y approuvés | Z rejetés

- **Nouveaux composants :**
  - `Components/Admin/Cards/CertificateCard.razor`
  - `Components/Admin/Cards/CertificateTable.razor`

---

## TODO — 4. Page admin Newsletter (Abonnés)

**Route :** `/admin/newsletter`
**Composant :** `Pages/Admin/Newsletter/Newsletter.razor`
**Layout :** `AdminLayout`

### Design

- **Header :** "Newsletter — Abonnés" + compteur "X abonnés"

- **Tableau :**
  | Colonne | Type |
  |---------|------|
  | Email | string |
  | Date inscription | datetime |
  | Statut | badge (Confirmé / En attente) |
  | Actions | dropdown : Supprimer |

- Export CSV (bouton facultatif)

---

## TODO — 5. Page admin Commentaires (Modération)

**Route :** `/admin/comments`
**Composant :** `Pages/Admin/Comments/Comments.razor`
**Layout :** `AdminLayout`

### Design

- **Filtres :**
  - Tous | En attente | Approuvés | Rejetés
  - Type : Articles | Événements
  - Recherche texte

- **Tableau :**
  | Colonne | Type |
  |---------|------|
  | Auteur | avatar + nom |
  | Contenu | tronqué 150 car. (lien détail) |
  | Type | badge Article/Événement |
  | Cible | titre (lien) |
  | Date | datetime |
  | Statut | badge orange/vert |
  | Actions | dropdown : Approuver, Supprimer |

---

## TODO — 6. Page admin Settings (Paramètres)

**Route :** `/admin/settings`
**Composant :** `Pages/Admin/Settings/Settings.razor`
**Layout :** `AdminLayout`

### Design

- **Header :** "Paramètres du site"

- **Tabs :**
  - `Général` | `Réseaux sociaux` | `Fonctionnalités` | `Notifications` | `Inscription`

- **Onglet Général :**
  - Nom du site (string)
  - URL du logo (upload image)
  - Email de contact (string)

- **Onglet Réseaux sociaux :**
  - Facebook URL, Twitter URL, LinkedIn URL, GitHub URL, YouTube URL (strings)

- **Onglet Fonctionnalités (toggles) :**
  - ✅ Approbation requise pour les événements
  - ✅ Approbation requise pour les articles
  - ✅ Inscription newsletter active
  - ✅ Upload public

- **Onglet Notifications :**
  - Email admin pour notifications (string)
  - Slack webhook URL (string, optionnel)

- **Onglet Inscription :**
  - Mode inscription : select (Ouverte / Invitation / Fermée)
  - Rôle par défaut des nouveaux : select (Collaborateur / Aucun)

- **Bouton global :** "Enregistrer les modifications"

---

## TODO — 7. Skills — Édition dans le profil

**Pages :**
- `/admin/profile/informations` (`Pages/Admin/Profile/EditProfile.razor`)
- `/profile/informations` (`Pages/Profile/EditProfile.razor`)

### Design

- Tag input : input text + bouton "Ajouter" → badge
- Entrée / clic "Ajouter" → ajoute le skill comme badge
- Clic sur ✗ du badge → supprime le skill
- `SyncFormFromState()` charge `List<string> Skills` depuis le UserDto
- Sauvegarde envoie `Skills` dans le payload

- **Nouveau composant :** `Components/Shared/SkillsEditor.razor`

---

## TODO — 8. Dashboard — Enrichissement

**Fichier :** `Pages/Admin/Dashboard.razor`

### Ajouts

- **Nouvelle KPI :** "Certificats en attente" (icône `fa-certificate`, orange)
- **Nouvelle section :** "Validations récentes" → liste 5 certificats + lien "Voir tout" vers `/admin/certificates`
- **Nouveau compteur KPI :** "Abonnés newsletter" (icône `fa-envelope`)

---

## TODO — 9. Sidebar — Nouveaux liens

**Fichier :** `Components/Admin/Shared/Sidebar.razor`

### À ajouter

| Icône | Label | Route |
|-------|-------|-------|
| `fa-tags` | Catégories | `/admin/categories` |
| `fa-tag` | Tags | `/admin/tags` |
| `fa-certificate` | Certificats | `/admin/certificates` |
| `fa-comments` | Commentaires | `/admin/comments` |
| `fa-envelope` | Newsletter | `/admin/newsletter` |
| `fa-gear` | Paramètres | `/admin/settings` |

---

## TODO — 10. Filtre Blog — Ajouter "En attente"

**Fichier :** `Pages/Admin/Blog/Blog.razor`

- Ajouter l'option "En attente" dans le `<select>` de filtre statut

---

## TODO — 11. Filtre Événements — Ajouter "En attente"

**Fichier :** `Pages/Admin/Event/Event.razor`

- Ajouter un onglet/filtre "En attente" (à côté de "Publiés", "Brouillons")

---

## TODO — 12. Composants Blazor à créer

| Composant | Chemin | Description |
|-----------|--------|-------------|
| `SkillsEditor.razor` | `Components/Shared/` | Tag input skills avec badges |
| `CertificateCard.razor` | `Components/Admin/Cards/` | Carte certificat avec approve/reject |
| `CertificateTable.razor` | `Components/Admin/Cards/` | Tableau certificats |
| `SettingsForm.razor` | `Components/Admin/Shared/` | Formulaire paramètres avec tabs |
