# Bugs Frontend

Bugs identifies dans les formulaires d'edition Blazor WASM.

## Event (`EventEdit.razor` + `MyEvents/EventEdit.razor`)

- `TagNames` jamais peuple -> tags effaces
- `OrganizerName` jamais envoye
- `IsPublished` toujours `true`

## Blog (`BlogEdit.razor` + `MyBlog/BlogEdit.razor`)

- `CategoryIds` non editable
- `IsPublished` toujours `true`

## Resource (`RessourceEdit.razor`)

- `CategoryIds` jamais peuple
- Tags non modifiables

## Backend

Le backend ignore `TagNames`, `TagIds`, `CategoryIds` si `null` (ne les efface plus). Le frontend doit les envoyer pour que l'update fonctionne.
