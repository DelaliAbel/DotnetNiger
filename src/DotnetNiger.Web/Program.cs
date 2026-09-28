using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Infrastructure;
using DotnetNiger.Web;
using DotnetNiger.Web.Authentication;
using DotnetNiger.Web.Administration;
using DotnetNiger.Web.Components;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

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

// Les contrôleurs de l'API sont découverts par le conteneur : les routes sont portées par
// les attributs [Route]/[HttpGet] de chaque contrôleur, sans préfixe de versionnement.
builder.Services.AddControllers();

// ---------- API : réponses d'erreur normalisées ----------
// RFC 9457 : le format d'erreur natif d'ASP.NET Core, plutôt qu'une enveloppe maison.
// Le gestionnaire traduit une règle métier refusée en 4xx au lieu de la laisser remonter
// en 500, qui ferait croire à une panne et exposerait une pile d'appels.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

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

    // Un formulaire public déclenche deux courriels par appel. La cadence de l'API, pensée
    // pour des lectures, laisserait un script remplir la file et la boîte de l'administration.
    options.AddPolicy("formulaire", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // Un essai de modèle met un vrai courriel en file. La cadence de l'API, faite pour des
    // lectures, laisserait un appel en boucle remplir la file de messages identiques.
    options.AddPolicy("courriel", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "inconnu",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
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

// ---------- Courriels : signaler une configuration incomplète ----------
// Un serveur SMTP renseigné sans expéditeur ni adresse du site laisse l'application
// démarrer en silence alors que plus aucun courriel ne part. Le membre ne le verrait qu'au
// moment de confirmer son inscription, et l'équipe ne le découvrirait qu'en production.
// Mieux vaut un avertissement au démarrage.
var optionsCourriel = app.Services.GetRequiredService<IOptions<EmailOptions>>().Value;
if (optionsCourriel.IsMisconfigured)
{
    StartupLog.CourrielsDesactives(
        app.Logger,
        string.Join(", ", optionsCourriel.DescribeMissingSettings()));
}
else if (optionsCourriel.IsConfigured)
{
    StartupLog.CourrielsActifs(app.Logger);
}

// ---------- Schéma de base de données ----------
// En production la stratégie est « None » : la migration a été appliquée pendant le
// déploiement, avec sauvegarde préalable. En développement local, EnsureCreated crée le
// schéma à partir du modèle sur une base SQLite jetable.
await app.Services.InitializeDatabaseAsync(app.Configuration);

// ---------- Rôles et premier administrateur ----------
// Idempotent : le même code peut s'exécuter à chaque démarrage.
await app.Services.GetRequiredService<IdentityBootstrapper>().InitializeAsync();

// ---------- Pipeline ----------
// Doit précéder la redirection HTTPS et l'authentification pour qu'une règle métier
// refusée par l'API soit traduite, quel que soit le mode d'exécution.
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// La page « introuvable » est un composant Razor : la ré-exécution ne convient qu'aux
// pages du site. Sur l'API, un 404 doit rester un 404 en JSON, et surtout la ré-exécution
// transformerait une réponse d'API en document HTML.
app.UseWhen(
    contexte => !EstAppelApi(contexte.Request),
    branche => branche.UseStatusCodePagesWithReExecute(
        "/not-found",
        createScopeForStatusCodePages: true));

app.UseHttpsRedirection();

// ---------- Authentification ----------
// L'authentification précède l'autorisation, et les deux précèdent les points de terminaison.
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
// Le middleware est posé pour les pages et composants du site, qui naviguent réellement
// et portent un jeton. Il est écarté de l'API : une requête JSON n'a pas de jeton de
// formulaire à présenter, et l'exiger rejetterait tout appel par fetch avec un 400. La
// contre-mesure contre la forge de requête intersite reste le cookie SameSite=Lax, ajouté
// par la configuration Identity, qui ne joint pas le cookie à un appel d'un site tiers.
app.UseWhen(
    contexte => !EstAppelApi(contexte.Request),
    branche => branche.UseAntiforgery());

app.MapStaticAssets();

// ---------- Parcours de compte ----------
app.MapExternalAccountEndpoints();
app.MapAccountEndpoints();
app.MapAccountVerificationEndpoints();

// ---------- Back-office ----------
app.MapAdministrationEndpoints();

// ---------- API ----------
// Préfixe unique, sans versionnement : le contrat n'a pas encore été publié à des clients
// qu'il serait coûteux de rompre.
app.MapControllers();

// ---------- Santé ----------
app.MapHealthChecks("/health").AllowAnonymous();

// ---------- Documentation ----------
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "DotnetNiger API");
    options.DocumentTitle = "DotnetNiger API";
});

// ---------- Front ----------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Indique si la requête vise l'API plutôt qu'une page du site.
//
// Le préfixe reprend celui porté par ApiRoutes.Base, que l'API est la seule à utiliser.
// L'API renvoie du JSON et des codes d'état : lui substituer une page HTML masquerait
// l'erreur auprès de l'appelant.
static bool EstAppelApi(HttpRequest requete) =>
    requete.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

/// <summary>
/// Point d'entrée exposé pour les tests d'intégration, qui utilisent
/// <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
public partial class Program;
