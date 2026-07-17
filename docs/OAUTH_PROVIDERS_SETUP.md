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

```
1. Utilisateur clique "Se connecter avec Google"
2. → Challenge redirect vers accounts.google.com
3. → Google redirige vers {IdentityBase}/signin-google
4. → Middleware ASP.NET Core traite le callback
5. → Redirige vers le handler (Razor Pages ou API)
6. → AuthService.HandleExternalLoginAsync()
     - Si email existe déjà → link le login externe
     - Sinon → crée un nouveau compte avec EmailConfirmed=true
7. → Retourne un ticket dans le cache mémoire (5 min)
8. → Frontend échange le ticket via POST /connect/token (grant_type=external_login)
```

### Fichiers clés

| Fichier | Rôle |
|---------|------|
| `DotnetNiger.Identity/Api/Extensions/ExternalAuthExtensions.cs` | Enregistrement des providers Google, GitHub, Microsoft |
| `DotnetNiger.Identity/Api/Controllers/ExternalAuthController.cs` | Endpoints API `/api/v1/auth/external-login`, `/external-callback`, `/external-callback-frontend` |
| `DotnetNiger.Identity/Pages/Account/Login.cshtml.cs` | Page de login Razor avec boutons sociaux |
| `DotnetNiger.Identity/Application/Services/AuthService.cs` | Logique métier : link, create user, generate ticket |
| `DotnetNiger.Identity/Api/Controllers/TokenController.cs` | Endpoint `/connect/token` (échange du ticket) |

### Activer/désactiver un provider

Chaque provider est activé **uniquement si** `Authentication:{Provider}:ClientId` est non vide dans la config. Pour désactiver un provider, mets la valeur à `""` ou supprime la section.
