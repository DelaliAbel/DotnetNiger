# Interconnexion Blazor WASM ↔ DotnetNiger API

Ce guide explique comment connecter un frontend **Blazor WebAssembly** au backend DotnetNiger
via l'**API Gateway** (port 5000).

---

## Architecture de communication

```
Blazor WASM (browser)
        │
        │  HTTP/HTTPS via HttpClient
        ▼
API Gateway — http://localhost:5000
        ├── /api/auth/*          → Identity API (port 5075)
        ├── /api/users/*         → Identity API (port 5075)
        ├── /api/community/posts/*    → Community API (port 5269)
        ├── /api/community/events/*   → Community API (port 5269)
        └── ...
```

> **Toujours passer par le Gateway**, jamais appeler directement les microservices.

---

## 1. Créer le projet Blazor WASM

```bash
dotnet new blazorwasm -o DotnetNiger.Web --no-https
cd DotnetNiger.Web
dotnet add package Microsoft.AspNetCore.Components.WebAssembly.Authentication
```

---

## 2. Configurer l'URL du Gateway (`wwwroot/appsettings.json`)

```json
{
  "ApiGateway": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

En production, remplace `http://localhost:5000` par l'URL publique du Gateway.

---

## 3. Enregistrer le `HttpClient` typé (`Program.cs` Blazor)

```csharp
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Lire l'URL du Gateway depuis la configuration
var gatewayUrl = builder.Configuration["ApiGateway:BaseUrl"] ?? "http://localhost:5000";

// Client anonyme (routes publiques)
builder.Services.AddHttpClient("GatewayPublic", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
});

// Client authentifié (routes protégées par JWT)
builder.Services.AddHttpClient("GatewayAuth", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
});

// Services métier (voir section 5)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();

// Gestion du token JWT en mémoire (localStorage ou sessionStorage)
builder.Services.AddScoped<TokenStore>();

await builder.Build().RunAsync();
```

---

## 4. Stocker et transmettre le JWT

### `TokenStore.cs`

```csharp
using Microsoft.JSInterop;

public class TokenStore
{
    private readonly IJSRuntime _js;

    public TokenStore(IJSRuntime js) => _js = js;

    public async Task SetTokenAsync(string token)
        => await _js.InvokeVoidAsync("localStorage.setItem", "jwt_token", token);

    public async Task<string?> GetTokenAsync()
        => await _js.InvokeAsync<string?>("localStorage.getItem", "jwt_token");

    public async Task RemoveTokenAsync()
        => await _js.InvokeVoidAsync("localStorage.removeItem", "jwt_token");
}
```

### `AuthenticatedHttpHandler.cs` — Injecter le Bearer token automatiquement

```csharp
public class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;

    public AuthenticatedHttpHandler(TokenStore tokenStore)
        => _tokenStore = tokenStore;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenStore.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
```

Enregistrer dans `Program.cs` Blazor :

```csharp
builder.Services.AddScoped<AuthenticatedHttpHandler>();
builder.Services.AddScoped<TokenStore>();

builder.Services.AddHttpClient("GatewayAuth", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
})
.AddHttpMessageHandler<AuthenticatedHttpHandler>();
```

---

## 5. Services métier Blazor

### `AuthService.cs` — Login / Logout

```csharp
using System.Net.Http.Json;

public class AuthService
{
    private readonly IHttpClientFactory _factory;
    private readonly TokenStore _tokenStore;

    public AuthService(IHttpClientFactory factory, TokenStore tokenStore)
    {
        _factory = factory;
        _tokenStore = tokenStore;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var client = _factory.CreateClient("GatewayPublic");
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result?.AccessToken is null) return false;

        await _tokenStore.SetTokenAsync(result.AccessToken);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.RemoveTokenAsync();
    }
}

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
```

### `CommunityService.cs` — Appels Community

```csharp
using System.Net.Http.Json;

public class CommunityService
{
    private readonly IHttpClientFactory _factory;

    public CommunityService(IHttpClientFactory factory) => _factory = factory;

    // Lecture publique
    public async Task<List<PostDto>?> GetPostsAsync(int page = 1, int pageSize = 10)
    {
        var client = _factory.CreateClient("GatewayPublic");
        return await client.GetFromJsonAsync<List<PostDto>>(
            $"/api/community/posts?page={page}&pageSize={pageSize}");
    }

    // Écriture (JWT requis)
    public async Task<bool> CreatePostAsync(CreatePostRequest request)
    {
        var client = _factory.CreateClient("GatewayAuth");
        var response = await client.PostAsJsonAsync("/api/community/posts", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<EventDto>?> GetEventsAsync()
    {
        var client = _factory.CreateClient("GatewayPublic");
        return await client.GetFromJsonAsync<List<EventDto>>("/api/community/events");
    }
}
```

---

## 6. Composant Blazor d'exemple — Liste des posts

```razor
@page "/posts"
@inject CommunityService CommunityService

<h2>Articles de la communauté</h2>

@if (_posts is null)
{
    <p>Chargement...</p>
}
else
{
    @foreach (var post in _posts)
    {
        <div class="post-card">
            <h3>@post.Title</h3>
            <p>@post.Summary</p>
        </div>
    }
}

@code {
    private List<PostDto>? _posts;

    protected override async Task OnInitializedAsync()
    {
        _posts = await CommunityService.GetPostsAsync();
    }
}
```

---

## 7. Composant Login

```razor
@page "/login"
@inject AuthService AuthService
@inject NavigationManager Nav

<h2>Connexion</h2>

<EditForm Model="_model" OnValidSubmit="HandleLogin">
    <DataAnnotationsValidator />
    <InputText @bind-Value="_model.Email" placeholder="Email" />
    <InputText type="password" @bind-Value="_model.Password" placeholder="Mot de passe" />
    <button type="submit">Se connecter</button>
    @if (_error is not null) { <p style="color:red">@_error</p> }
</EditForm>

@code {
    private LoginModel _model = new();
    private string? _error;

    private async Task HandleLogin()
    {
        var ok = await AuthService.LoginAsync(_model.Email, _model.Password);
        if (ok)
            Nav.NavigateTo("/");
        else
            _error = "Email ou mot de passe incorrect.";
    }

    class LoginModel
    {
        [Required] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }
}
```

---

## 8. CORS — Configuration côté Gateway

Le CORS est déjà configuré dans le Gateway pour `AllowAll` en développement.
En **production**, restreindre à l'URL Blazor dans `Program.cs` du Gateway :

```csharp
options.AddPolicy("AllowBlazor", policy =>
    policy.WithOrigins("https://votre-domaine.com")
          .AllowAnyMethod()
          .AllowAnyHeader());
```

---

## 9. Tableau récapitulatif des endpoints Gateway

| Fonctionnalité       | Méthode | URL Gateway                   | Auth requise |
| -------------------- | ------- | ----------------------------- | :----------: |
| Login                | POST    | `/api/auth/login`             |     Non      |
| Register             | POST    | `/api/auth/register`          |     Non      |
| Refresh token        | POST    | `/api/auth/refresh`           |     Non      |
| Profil utilisateur   | GET     | `/api/users/me`               |     JWT      |
| Modifier profil      | PUT     | `/api/users/me`               |     JWT      |
| Liste des posts      | GET     | `/api/community/posts`        |     Non      |
| Créer un post        | POST    | `/api/community/posts`        |     JWT      |
| Liste des événements | GET     | `/api/community/events`       |     Non      |
| Créer un événement   | POST    | `/api/community/events`       |     JWT      |
| Liste des projets    | GET     | `/api/community/projects`     |     Non      |
| Créer un projet      | POST    | `/api/community/projects`     |     JWT      |
| Ressources           | GET     | `/api/community/resources`    |     Non      |
| Catégories           | GET     | `/api/community/categories`   |     Non      |
| Tags                 | GET     | `/api/community/tags`         |     Non      |
| Partenaires          | GET     | `/api/community/partners`     |     Non      |
| Recherche            | GET     | `/api/community/search?q=...` |     Non      |
| Stats                | GET     | `/api/community/stats`        |     Non      |
| Health check         | GET     | `/health`                     |     Non      |

---

## 10. Ordre de démarrage des services

```bash
# Terminal 1 — Identity API
cd DotnetNiger.Identity
dotnet run

# Terminal 2 — Community API
cd DotnetNiger.Community
dotnet run

# Terminal 3 — Gateway (après que les deux APIs soient démarrées)
cd DotnetNiger.Gateway
dotnet run

# Terminal 4 — Blazor WASM
cd DotnetNiger.Web
dotnet run
```

- Identity : http://localhost:5075/swagger
- Community : http://localhost:5269/swagger
- Gateway : http://localhost:5000/swagger
- Blazor WASM : http://localhost:5001 (ou le port attribué)

---

## 11. Gestion des erreurs HTTP dans Blazor

```csharp
public async Task<ApiResult<T>> SafeGetAsync<T>(string url)
{
    try
    {
        var client = _factory.CreateClient("GatewayPublic");
        var response = await client.GetAsync(url);

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.OK =>
                ApiResult<T>.Ok(await response.Content.ReadFromJsonAsync<T>()),
            System.Net.HttpStatusCode.Unauthorized =>
                ApiResult<T>.Fail("Session expirée, reconnectez-vous."),
            System.Net.HttpStatusCode.NotFound =>
                ApiResult<T>.Fail("Ressource introuvable."),
            _ => ApiResult<T>.Fail($"Erreur serveur ({(int)response.StatusCode}).")
        };
    }
    catch (HttpRequestException)
    {
        return ApiResult<T>.Fail("Impossible de joindre le serveur.");
    }
}

public record ApiResult<T>(bool Success, T? Data, string? Error)
{
    public static ApiResult<T> Ok(T? data) => new(true, data, null);
    public static ApiResult<T> Fail(string error) => new(false, default, error);
}
```
