# Configuration OAuth — Google, GitHub, Microsoft

## Architecture

L'Identity service tourne à ces URLs :
- **Développement** : `http://localhost:5075`
- **Production** : `https://identity-dotnetniger.runasp.net`

Le frontend SPA est à :
- **Développement** : `http://localhost:5201`
- **Production** : `https://dotnetniger.vercel.app`

---

## Google

### 1. Console Google Cloud
https://console.cloud.google.com/apis/credentials

- Créer un projet (ou utiliser un existant)
- Aller dans **APIs & Services > Credentials**
- **Create Credentials > OAuth 2.0 Client ID**
- Application type : **Web application**
- Ajouter les **Authorized redirect URIs** :

| Environnement | URI |
|--------------|-----|
| Développement | `http://localhost:5075/signin-google` |
| Production | `https://identity-dotnetniger.runasp.net/signin-google` |

### 2. Configurer dans l'app

**Développement** (`appsettings.json` ou User Secrets) :
```json
"Authentication": {
    "Google": {
        "ClientId": "__TON_CLIENT_ID_GOOGLE__",
        "ClientSecret": "__TON_CLIENT_SECRET_GOOGLE__"
    }
}
```

**Production** (`appsettings.Production.json`) :
```json
"Authentication": {
    "Google": {
        "ClientId": "__TON_CLIENT_ID_GOOGLE__",
        "ClientSecret": "__TON_CLIENT_SECRET_GOOGLE__"
    }
}
```

---

## GitHub

### 1. GitHub OAuth App
https://github.com/settings/developers

- Aller dans **Settings > Developer settings > OAuth Apps > New OAuth App**
- Remplir :
  - **Application name** : `DotnetNiger`
  - **Homepage URL** : `https://dotnetniger.vercel.app` (ou `http://localhost:5201` en dev)
  - **Authorization callback URL** : (voir tableau ci-dessous)

| Environnement | Callback URL |
|--------------|-------------|
| Développement | `http://localhost:5075/signin-github` |
| Production | `https://identity-dotnetniger.runasp.net/signin-github` |

- Enregistrer l'app, puis copier le **Client ID** et générer un **Client Secret**

### 2. Configurer dans l'app

**Développement** (`appsettings.json` ou User Secrets) :
```json
"Authentication": {
    "GitHub": {
        "ClientId": "__TON_CLIENT_ID_GITHUB__",
        "ClientSecret": "__TON_CLIENT_SECRET_GITHUB__"
    }
}
```

**Production** (`appsettings.Production.json`) :
```json
"Authentication": {
    "GitHub": {
        "ClientId": "__TON_CLIENT_ID_GITHUB__",
        "ClientSecret": "__TON_CLIENT_SECRET_GITHUB__"
    }
}
```

---

## Microsoft

### 1. Azure Portal
https://portal.azure.com > **App registrations**

- **New registration**
  - **Name** : `DotnetNiger`
  - **Supported account types** : `Accounts in any organizational directory (Any Microsoft Entra ID tenant - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)`
  - **Redirect URI** : Web — voir tableau ci-dessous

| Environnement | Redirect URI |
|--------------|-------------|
| Développement | `http://localhost:5075/signin-microsoft` |
| Production | `https://identity-dotnetniger.runasp.net/signin-microsoft` |

- Après création, copier le **Application (client) ID**
- Aller dans **Certificates & secrets > New client secret** et copier la **Value** (c'est le ClientSecret)

### 2. Configurer dans l'app

**Développement** (`appsettings.json` ou User Secrets) :
```json
"Authentication": {
    "Microsoft": {
        "ClientId": "__TON_CLIENT_ID_MICROSOFT__",
        "ClientSecret": "__TON_CLIENT_SECRET_MICROSOFT__"
    }
}
```

**Production** (`appsettings.Production.json`) :
```json
"Authentication": {
    "Microsoft": {
        "ClientId": "__TON_CLIENT_ID_MICROSOFT__",
        "ClientSecret": "__TON_CLIENT_SECRET_MICROSOFT__"
    }
}
```

> ⚠️ **Important** : Actuellement, Microsoft n'est **pas configuré en production**. Les valeurs sont des placeholders. Remplace-les par les vraies credentials Azure.

---

## Détails techniques (pour référence)

### Callback paths par provider

| Provider | CallbackPath (framework) |
|---------|------------------------|
| Google | `/signin-google` (par défaut ASP.NET Core) |
| GitHub | `/signin-github` (explicite dans `ExternalAuthExtensions.cs:73`) |
| Microsoft | `/signin-microsoft` (par défaut ASP.NET Core) |

### Flux OAuth complet

Le login ET le register (création de compte) passent par les mêmes pages Razor
servies par **Identity via le Gateway** (`/Account/Login`, `/Account/Register`).

```
1. Utilisateur est sur dotnetniger.vercel.app
2. Clique "Connexion" ou "S'inscrire"
   → redirection vers identity-dotnetniger.runasp.net/Account/Login?returnUrl=/auth/external-callback
   (ou /Account/Register)

3. Sur la page Login/Register, clique "Google/GitHub/Microsoft"
   → Challenge redirect vers accounts.google.com (etc.)

4. Provider redirige vers identity-dotnetniger.runasp.net/signin-{provider}
   (via Gateway, route Ocelot dédiée)

5. Middleware ASP.NET Core traite le callback
   → Redirect vers /Account/Login?handler=ExternalCallback

6. AuthService.HandleExternalLoginAsync() gère 3 cas :
   - Compte déjà lié au provider → connexion directe
   - Email existe déjà en BDD mais pas lié → lie le login externe au compte
   - Nouvel email → crée un nouveau compte User avec EmailConfirmed=true

7. Retourne un ticket unique (1 use, 5 min TTL) dans le cache mémoire
   → Redirection vers dotnetniger.vercel.app/auth/external-callback?ticket=xxx

8. Frontend (ExternalCallback.razor) reçoit le ticket
   → POST /connect/token avec grant_type=external_login&ticket=xxx
   → Reçoit access_token + refresh_token
   → Access token stocké en mémoire C# (INACCESSIBLE au JS)
   → Refresh token stocké en localStorage (clé obfusquée)
   → Utilisateur connecté ✅
```

### Différence Login vs Register

| Page | Boutons sociaux | Comportement |
|------|----------------|-------------|
| Login | Google, GitHub, Microsoft | Connecte ou crée le compte si nouveau |
| Register | Google, GitHub, Microsoft | Redirige vers Login → idem (crée si nouveau) |

Le Register sert surtout pour l'inscription par email/mot de passe. Les boutons
sociaux sur Register redirigent vers le handler ExternalLogin du Login — donc
le flux est identique : si l'utilisateur n'existe pas, il est créé automatiquement.

---

## Sécurité frontend

### Stockage des tokens (Blazor WASM)

| Donnée | Stockage | Clé localStorage | Accessible au JS |
|--------|----------|-----------------|-----------------|
| Access token (JWT) | Mémoire C# (variable privée) | `dn_wasm_runtime_registry_key` | ❌ Non — mémoire WASM isolée |
| Refresh token | localStorage | `dn_wasm_runtime_registry_renew` | ✅ Oui mais inutilisable seul |
| Profil utilisateur | localStorage | `dn_wasm_runtime_registry_member` | ✅ Oui (infos non sensibles) |
| Client ID navigateur | localStorage | `dn_wasm_runtime_registry_client` | ✅ Oui |

Le refresh token seul ne peut pas être échangé sans le client_id, et ne donne
pas accès aux API. L'access token (JWT) n'est jamais exposé au JavaScript car
il reste dans une variable C# privée du WebAssembly.

### Content-Security-Policy (CSP)

Une balise `<meta http-equiv="Content-Security-Policy">` est présente dans
`wwwroot/index.html` pour limiter les vecteurs XSS :

```
default-src 'self'
script-src 'self' 'unsafe-inline' 'wasm-unsafe-eval'
style-src 'self' 'unsafe-inline'
font-src 'self' data:
connect-src 'self' https://dotnetniger.runasp.net http://localhost:5000
img-src 'self' data: https:
base-uri 'self'
form-action 'self'
```

---

## Fichiers clés

| Fichier | Rôle |
|---------|------|
| **Backend (DotnetNiger.Identity)** | |
| `Api/Extensions/ExternalAuthExtensions.cs` | Enregistrement des providers Google, GitHub, Microsoft |
| `Api/Controllers/ExternalAuthController.cs` | Endpoints API `/api/v1/auth/external-login`, `/external-callback`, `/external-callback-frontend` |
| `Pages/Account/Login.cshtml.cs` | Page de login Razor avec boutons sociaux |
| `Pages/Account/Register.cshtml.cs` | Page de register avec boutons sociaux |
| `Application/Services/AuthService.cs` | Logique métier : link, create user, generate ticket |
| `Api/Controllers/TokenController.cs` | Endpoint `/connect/token` (échange du ticket) |
| **Frontend (UIgit — Blazor WASM)** | |
| `Pages/Auth/ExternalCallback.razor` | Reçoit le ticket depuis l'URL, déclenche l'échange |
| `Services/Auth/AuthService.cs` | CompleteExternalLoginAsync → POST /connect/token |
| `Services/Auth/CustomAuthStateProvider.cs` | Stockage mémoire + localStorage des tokens |
| `Services/Auth/ClientIdHeaderHandler.cs` | Injecte Bearer JWT dans chaque requête API |
| `Pages/Auth/Login.razor` | Page de login frontend (redirige vers Identity) |

---

## Activer/désactiver un provider

Chaque provider est activé **uniquement si** `Authentication:{Provider}:ClientId`
est non vide dans la config. Pour désactiver un provider, mets la valeur à `""`
ou supprime la section.
