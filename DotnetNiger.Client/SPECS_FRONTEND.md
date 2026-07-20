# Specifications Frontend

## Dashboard Collaborateur

| Role | Comportement |
|------|-------------|
| Admin / SuperAdmin | Dashboard stats globales |
| Collaborator | Dashboard stats personnelles |
| User | Redirection vers `/admin/profile` |

## Permissions

Composant `<PermissionDenied />` pour les utilisateurs sans permission requise.

## Pages

- **Projets** : `/projets` avec grille responsive, filtres, pagination
- **Permissions** : Page admin reservee aux SuperAdmin

## Formulaires edition

| Formulaire | Champs a binder |
|------------|----------------|
| `EventEdit.razor` | `TagNames`, `OrganizerName`, `IsPublished` |
| `BlogEdit.razor` | `CategoryIds`, `SeoDescription`, `IsPublished` |
| `RessourceEdit.razor` | `CategoryIds`, `TagNames` |
