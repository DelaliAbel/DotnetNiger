# Plan de Conception — DotnetNiger.Identity

**Target :** .NET 9.0 | **Pattern :** Clean Architecture (light) | **Auth :** Identity + OpenIddict + Social

---

## 1. Principe Fondamental

> **NE RIEN RÉINVENTER.** Tout ce qu'ASP.NET Core Identity fournit déjà est utilisé directement :
> - `UserManager<TUser>` → users (CRUD, email, password)
> - `RoleManager<TRole>` → roles
> - `SignInManager<TUser>` → login/logout
> - `IEmailSender<TUser>` → email
> - `AddAuthentication().AddGoogle().AddMicrosoftAccount().AddOAuth(...)` → social login

On étend UNIQUEMENT ce qui manque : **multi-tenant** + **permissions par tenant** + **OpenIddict**.

---

## 2. Architecture

```
Api (Controllers)
  ↓ Appelle directement
Application (Services)
  ↓ Utilise
Domain (Entités)
```

```
Infrastructure (DbContext, Seeds, Email, TenantResolution)
  ↓ Implémente les besoins des Services
```

**Pas de Repository** — UserManager/RoleManager/SignInManager sont utilisés directement.

---

## 3. Domain — Entités

**4 entités seulement :**

```
Domain/Entities/
├── ApplicationUser.cs   → IdentityUser<Guid> + TenantId + FullName + IsActive
├── ApplicationRole.cs   → IdentityRole<Guid> + TenantId
├── Tenant.cs            → Id + Name + Slug + IsActive
└── Permission.cs        → Id + TenantId + Name + Category

(ApplicationUserRole = classe jointe Identity standard)
```

**ApplicationUser :**
```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**ApplicationRole :**
```csharp
public class ApplicationRole : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
    public string? Description { get; set; }
}
```

**Tenant :**
```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
```

**Permission :**
```csharp
public class Permission
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;   // "user.read"
    public string Category { get; set; } = string.Empty; // "User", "Role", "Admin"
}
```

---

## 4. Application — Services (≤ 200 lignes chacun)

6 services, chacun < 200 lignes, utilisant les managers Identity :

### AuthService (~180 lignes)
- `LoginAsync(email, password, tenantId)` — SignInManager.PasswordSignInAsync + OpenIddict token
- `ExternalLoginCallbackAsync()` — gère le retour OAuth Google/Microsoft/GitHub
- `RefreshTokenAsync(refreshToken)` — OpenIddict refresh
- `LogoutAsync()` — SignInManager.SignOutAsync

### UserService (~190 lignes)
- CRUD via UserManager (CreateAsync, FindByIdAsync, UpdateAsync, DeleteAsync)
- ChangePassword, ForgotPassword, ResetPassword, ConfirmEmail via UserManager
- Filtre par TenantId après lookup

### RoleService (~160 lignes)
- CRUD via RoleManager
- AssignRole/RemoveRole via UserManager.AddToRoleAsync / RemoveFromRoleAsync
- GetRolesByTenant, GetUserRoles

### PermissionService (~170 lignes)
- CRUD Permission via DbContext directement (pas de manager Identity pour ça)
- AssignPermissionToRole / RemovePermissionFromRole
- GetUserPermissions — jointure User → Role → RolePermission → Permission

### TenantService (~150 lignes)
- CRUD Tenant via DbContext
- Lookup by Slug, Activate/Suspend

### AdminService (~150 lignes)
- Super admin : GetAllTenants, GetAllUsersAcrossTenants
- System stats (count users, tenants, roles)

---

## 5. DTOs — 10 fichiers max

```
Application/DTOs/
├── AuthRequests.cs      → LoginRequest, RefreshTokenRequest, ExternalLoginRequest
├── AuthResponses.cs     → TokenResponse, UserInfoResponse
├── UserRequests.cs      → CreateUserRequest, UpdateUserRequest, ChangePasswordRequest
├── UserResponses.cs     → UserResponse, UserProfileResponse
├── RoleRequests.cs      → CreateRoleRequest, UpdateRoleRequest
├── RoleResponses.cs     → RoleResponse, RoleWithPermissionsResponse
├── PermissionRequests.cs → CreatePermissionRequest
├── PermissionResponses.cs → PermissionResponse
├── TenantRequests.cs    → CreateTenantRequest, UpdateTenantRequest
└── Common.cs            → PaginatedResponse<T>, ErrorResponse
```

Mapping manuel via méthodes d'extension (pas d'AutoMapper).

---

## 6. Infrastructure — DbContext

**IdentityDbContext** configure :
- ASP.NET Identity tables (users, roles, claims, etc.)
- Tables custom (Tenants, Permissions, RolePermission)
- **Query filter global** sur TenantId pour l'isolation
- Index unique (TenantId + PermissionName)

**TenantResolutionService** — résout le tenant depuis :
1. Header `X-Tenant-Id` (pour les apps internes)
2. Claim `tenant_id` dans le JWT
3. Sous-domaine (optionnel)

**Seeds** (tout dans `DbSeeder.cs`) :
- SuperAdmin (admin@dotnetniger.com)
- Default roles (Admin, User)
- Default permissions (user.read, user.write, role.manage, etc.)

---

## 7. Social Login — Google + Microsoft + GitHub

Configuration dans Program.cs :

```csharp
services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = config["Google:ClientId"];
        options.ClientSecret = config["Google:ClientSecret"];
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = config["Microsoft:ClientId"];
        options.ClientSecret = config["Microsoft:ClientSecret"];
    })
    .AddOAuth("GitHub", options =>
    {
        options.ClientId = config["GitHub:ClientId"];
        options.ClientSecret = config["GitHub:ClientSecret"];
        options.CallbackPath = "/signin-github";
        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
        options.UserInformationEndpoint = "https://api.github.com/user";
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    });
```

**Flow :** `POST /api/v1/auth/external-login?provider=Google` → redirect → callback → SignInManager.ExternalLoginSignInAsync → lien ou création user → token JWT.

---

## 8. OpenIddict — OAuth2/OIDC

```csharp
services.AddOpenIddict()
    .AddCore(opts => opts.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())
    .AddServer(opts =>
    {
        opts.SetTokenEndpointUris("/connect/token");
        opts.AllowPasswordFlow().AllowRefreshTokenFlow();
        opts.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
        opts.UseAspNetCore().EnableTokenEndpointPassthrough();
        opts.RegisterScopes("openid", "email", "profile", "roles", "api");
    })
    .AddValidation(opts => { opts.UseLocalServer(); opts.UseAspNetCore(); });
```

---

## 9. API — 7 Controllers

| Controller | Routes | Auth |
|---|---|---|
| `AuthController` | POST login, refresh, logout, external-login, external-callback | Public / [Authorize] |
| `UsersController` | CRUD /api/v1/{tenantId}/users | [Authorize(Roles=Admin)] |
| `RolesController` | CRUD /api/v1/{tenantId}/roles | [Authorize(Roles=Admin)] |
| `PermissionsController` | CRUD /api/v1/{tenantId}/permissions | [Authorize(Roles=Admin)] |
| `TenantsController` | CRUD /api/v1/admin/tenants | [Authorize(Policy=SuperAdmin)] |
| `AdminController` | GET stats, logs | [Authorize(Policy=SuperAdmin)] |
| `ProfileController` | GET/PUT /api/v1/profile | [Authorize] |

---

## 10. Structure Finale des Fichiers (~30 fichiers)

```
DotnetNiger.Identity/
├── Domain/
│   └── Entities/
│       ├── ApplicationUser.cs
│       ├── ApplicationRole.cs
│       ├── Tenant.cs
│       └── Permission.cs
├── Application/
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── UserService.cs
│   │   ├── RoleService.cs
│   │   ├── PermissionService.cs
│   │   ├── TenantService.cs
│   │   └── AdminService.cs
│   ├── DTOs/
│   │   ├── AuthRequests.cs
│   │   ├── AuthResponses.cs
│   │   ├── UserRequests.cs
│   │   ├── UserResponses.cs
│   │   ├── RoleRequests.cs
│   │   ├── RoleResponses.cs
│   │   ├── PermissionRequests.cs
│   │   ├── PermissionResponses.cs
│   │   ├── TenantRequests.cs
│   │   └── Common.cs
│   └── Validators.cs
├── Infrastructure/
│   ├── IdentityDbContext.cs
│   ├── DesignTimeDbContextFactory.cs
│   ├── DbSeeder.cs
│   ├── EmailSender.cs
│   └── TenantResolutionService.cs
├── Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── RolesController.cs
│   │   ├── PermissionsController.cs
│   │   ├── TenantsController.cs
│   │   ├── AdminController.cs
│   │   └── ProfileController.cs
│   ├── Middleware/
│   │   ├── TenantResolutionMiddleware.cs
│   │   └── ErrorHandlingMiddleware.cs
│   └── ServiceExtensions.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── DotnetNiger.Identity.csproj
├── Dockerfile
└── Properties/
    └── launchSettings.json
```

---

## 11. Plan d'Exécution

| Phase | Ce qu'on fait |
|---|---|
| **1. Projet** | .csproj + packages + .editorconfig |
| **2. Domain** | 4 entités |
| **3. Infrastructure** | DbContext + seeds + TenantResolution |
| **4. Application** | 6 services + DTOs + validators |
| **5. API** | Controllers + middleware + Program.cs |
| **6. Build** | dotnet build + correction erreurs |

Chaque phase produit un état **compilable et fonctionnel** avant de passer à la suivante.
