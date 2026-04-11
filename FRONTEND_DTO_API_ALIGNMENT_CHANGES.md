# Frontend - Alignement API et DTOs

Date: 2026-03-27
Projet: DotnetNiger.UI

## Objectif

Mettre a jour le frontend pour:

- aligner les DTOs avec les contrats exposes par DotnetNiger.Identity et DotnetNiger.Community,
- fiabiliser la consommation des reponses API enveloppees (`ApiSuccessResponse<T>`),
- verifier/coordonner login, register et redirection admin.

## 1) Changements DTO (Frontend)

### 1.1 PostDto aligne sur Community API

Fichier modifie: `Models/Responses/PostDto.cs`

- Renomme `ContentHTML` -> `Content` pour correspondre au backend Community (`PostDto.Content`).

Impact:

- Evite la perte de contenu lors de la deserialisation des posts.

### 1.2 SearchResultDto aligne sur Community API

Fichier modifie: `Models/Responses/SearchResultDto.cs`

- Ajoute `Name`.
- Ajoute `Content`.
- Ajoute `Description`.
- Ajoute `StartDateTime`.

Impact:

- Le frontend peut maintenant exploiter les champs de recherche exposes par l'API (notamment event/time).

### 1.3 AuthDto aligne sur Identity API

Fichier modifie: `Models/Responses/AuthDto.cs`

- Supprime les champs frontend-only:
  - `Success`
  - `Message`
- Conserve le contrat backend:
  - `User`
  - `Token`

Impact:

- Le DTO est maintenant strictement conforme a l'API Identity.

## 2) Changements de consommation API (Frontend)

### 2.1 Lecture robuste des reponses API enveloppees

Fichier cree: `Services/Api/ApiResponseReader.cs`

- Ajoute un lecteur commun pour parser:
  - `ApiSuccessResponse<T>`
  - `ApiSuccessResponse<PaginatedDto<T>>`
  - fallback JSON direct

Impact:

- Uniformise la deserialisation dans tous les services API du front.

### 2.2 Services API alignes routes + enveloppes

Fichiers modifies:

- `Services/Api/ApiPostService.cs`
- `Services/Api/ApiEventService.cs`
- `Services/Api/ApiResourceService.cs`
- `Services/Api/ApiProfileService.cs`

Principales corrections:

- Parsing via `ApiResponseReader`.
- Alignement des endpoints Community utilises par le front.
- Correction des endpoints registration/cancel event pour suivre le contrat securise backend.

### 2.3 Injection automatique du bearer token

Fichiers modifies:

- `Services/Auth/ClientIdHeaderHandler.cs`
- `Program.cs`

Corrections:

- Ajout automatique du header `Authorization: Bearer <token>` si token disponible.
- Conservation du header `ClientId`.
- Mise a jour du wiring DI pour fournir `CustomAuthStateProvider` au handler.

## 3) Auth flow (login/register/reset/refresh)

### 3.1 AuthService adapte au contrat DTO backend

Fichier modifie: `Services/Auth/AuthService.cs`

Corrections principales:

- `LoginAsync`: continue de retourner `ApiSuccessResponse<AuthDto>`, avec lecture message erreur depuis JSON (`detail`/`message`/`error`).
- `RegisterAsync`: passe a `Task<ApiSuccessResponse<AuthDto>>` et sauvegarde les tokens depuis `result.Data.Token`.
- `RefreshTokenAsync`: sauvegarde les tokens des que `Token` est present dans `AuthDto`.
- `ResetPasswordAsync`: passe a `Task<ApiSuccessResponse<object>>` pour rester coherent avec les reponses API enveloppees.

### 3.2 Pages Auth adaptees

Fichiers modifies:

- `Pages/Auth/Login.razor`
- `Pages/Auth/Register.razor`
- `Pages/Auth/ResetPassword.razor`

Corrections:

- `Login`: message erreur null-safe.
- `Register`: lit `ApiSuccessResponse<AuthDto>` et redirige selon roles (`result.Data.User.Roles`).
- `ResetPassword`: lit `ApiSuccessResponse<object>`.

## 4) Redirection admin / roles

### 4.1 Autorisation des pages admin alignee roles API

Fichier modifie: `Pages/Admin/_Imports.razor`

- Passe de `Authorize(Roles = "admin")` a `Authorize(Roles = "Admin,SuperAdmin")`.

### 4.2 Topbar admin

Fichier modifie: `Components/Admin/Shared/Topbar.razor`

- Role affiche de facon null-safe (`FirstOrDefault`).
- Bouton deconnexion branche a `AuthService.LogoutAsync()` puis redirection `/login`.

## 5) Adaptation d'usage apres renommage PostDto.Content

Fichier modifie: `Pages/Admin/Blog/BlogEdit.razor`

- `post.ContentHTML` remplace par `post.Content`.

## 6) Notes de validation

- Les fichiers DTO/services/pages modifies ont ete verifies via diagnostics locaux fichier par fichier.
- Le workspace contient des changements backend/tests independants du front (non modifies ici).
- Les erreurs Razor signalees par l'analyseur autour de composants (`PageTitle`, `EditForm`, etc.) sont liees a la resolution design-time de l'environnement; des imports auth ont ete renforces dans:
  - `Pages/Auth/_Imports.razor`

## 7) Resume des fichiers frontend touches

- `Program.cs`
- `Models/Responses/AuthDto.cs`
- `Models/Responses/PostDto.cs`
- `Models/Responses/SearchResultDto.cs`
- `Services/Api/ApiResponseReader.cs` (nouveau)
- `Services/Api/ApiPostService.cs`
- `Services/Api/ApiEventService.cs`
- `Services/Api/ApiResourceService.cs`
- `Services/Api/ApiProfileService.cs`
- `Services/Auth/AuthService.cs`
- `Services/Auth/ClientIdHeaderHandler.cs`
- `Pages/Auth/Login.razor`
- `Pages/Auth/Register.razor`
- `Pages/Auth/ResetPassword.razor`
- `Pages/Auth/_Imports.razor`
- `Pages/Admin/_Imports.razor`
- `Pages/Admin/Blog/BlogEdit.razor`
- `Components/Admin/Shared/Topbar.razor`

## 8) Resultat attendu

- DTOs frontend critiques alignes avec l'API.
- Consommation API plus fiable (reponses enveloppees, headers, endpoints).
- Flux login/register/reset/refresh coherent avec le contrat backend.
- Redirection et acces admin coherents avec les roles Identity (`Admin`, `SuperAdmin`).

## 9) Correctif build UI apres renommage DTO Post

Contexte:

- Le renommage de `PostDto.ContentHTML` vers `PostDto.Content` a casse les mocks front qui utilisaient encore l'ancien nom.

Correctif applique:

- Fichier corrige: `Services/Mock/PostService.cs`
- Remplacement global des occurrences `ContentHTML` par `Content`.

Verification:

- Commande executee dans `DotnetNiger.UI`: `dotnet build`
- Resultat: build OK avec warnings non bloquants (CS8601 dans `Services/Mock/ProfileService.cs`).
