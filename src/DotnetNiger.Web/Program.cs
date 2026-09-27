using System.Threading.RateLimiting;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Web.Authentication;
using DotnetNiger.Web.Components;
using DotnetNiger.Web.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Infrastructure : persistance ----------
builder.Services.AddDotnetNigerPersistence(builder.Configuration);

// ---------- Web : état de requête et utilisateur courant ----------
// `AddHttpContextAccessor` est aussi requis par le service de rattachement des comptes
// externes, qui lit l'utilisateur de la session courante.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ExternalCallbackHandler>();
builder.Services.AddScoped<AccountOperations>();
builder.Services.AddSingleton<PendingExternalSignInProtector>();

// ---------- Comptes, rôles, mots de passe et cookies ----------
// Enregistré avant les couches Blazor et OAuth, qui dépendent de SignInManager.
builder.Services.AddDotnetNigerIdentity(builder.Configuration);

// ---------- Connexion par fournisseur externe ----------
// Le cookie Identity reste le schéma par défaut. Google, Microsoft et GitHub ne sont ajoutés
// que si un identifiant et un secret figurent dans la configuration : le site démarre sans
// eux plutôt que d'exposer un bouton qui échouerait.
builder.Services
    .AddAuthentication()
    .AddConfiguredExternalProviders(builder.Configuration);

builder.Services.AddAuthorization();

// ---------- Front Blazor ----------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ---------- API : documentation native .NET ----------
builder.Services.AddOpenApi();

// ---------- API : réponses d'erreur normalisées ----------
// RFC 9457 : le format d'erreur natif d'ASP.NET Core, plutôt qu'une enveloppe maison.
builder.Services.AddProblemDetails();

// ---------- Limitation de débit ----------
// Deux niveaux, comme dans le template de référence : global sur tout le site,
// resserré sur l'API, où chaque requête coûte une interrogation de base.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

// ---------- Santé : la base doit répondre avant de déclarer le site opérationnel ----------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DotnetNigerDbContext>("database");

var app = builder.Build();

// ---------- Schéma de base de données ----------
// En production la stratégie est « None » : la migration a été appliquée pendant le
// déploiement, avec sauvegarde préalable. En développement local, EnsureCreated crée le
// schéma à partir du modèle sur une base SQLite jetable.
await app.Services.InitializeDatabaseAsync(app.Configuration);

// ---------- Rôles et premier administrateur ----------
// Idempotent : le même code peut s'exécuter à chaque démarrage.
await app.Services.GetRequiredService<IdentityBootstrapper>().InitializeAsync();

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// ---------- Authentification ----------
// L'authentification précède l'autorisation, et les deux précèdent les points de terminaison.
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

// ---------- Parcours de compte ----------
app.MapExternalAccountEndpoints();
app.MapAccountEndpoints();
app.MapAccountVerificationEndpoints();

// ---------- Santé ----------
app.MapHealthChecks("/health").AllowAnonymous();

// ---------- Documentation ----------
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "dotnetNiger API");
    options.DocumentTitle = "dotnetNiger API";
});

// ---------- Front ----------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Point d'entrée exposé pour les tests d'intégration, qui utilisent
/// <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
public partial class Program;
