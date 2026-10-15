using DotnetNiger.Application.Abstractions;
using DotnetNiger.Infrastructure.Email;
using DotnetNiger.Infrastructure.Identity;
using DotnetNiger.Infrastructure.Persistence;
using DotnetNiger.Infrastructure;
using DotnetNiger.Web;
using DotnetNiger.Web.Authentication;
using DotnetNiger.Web.Services.Commentaires;
using DotnetNiger.Web.Services.Membres;
using DotnetNiger.Web.Services.Inscriptions;
using DotnetNiger.Web.Components;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Services;
using DotnetNiger.Web.Services.Ui;
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
builder.Services.AddScoped<ServiceInscriptionEvenement>();
builder.Services.AddScoped<ServiceCommentaires>();
builder.Services.AddScoped<ServiceMembres>();
builder.Services.AddScoped<DotnetNiger.Web.Services.EspaceMembre.ServiceEspaceMembre>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceTableauDeBord>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceBlog>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceEvenements>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceRessources>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceProjets>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServicePartenaires>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceCommentaires>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceSignalements>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceMessages>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceMembres>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceAbonnees>();
builder.Services.AddScoped<DotnetNiger.Web.Services.Administration.ServiceParametres>();
builder.Services.AddScoped<ServiceInscriptionsMembre>();
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

// ---------- Contenus du site public ----------
// Les pages lisent la base par les mêmes services que l'API : un contenu retiré côté
// écriture disparaît des deux côtés, et aucun écran ne peut annoncer un article que
// l'API ne renvoie pas.
builder.Services.AddScoped<ISiteContentService, ContenuSiteService>();

// ---------- API : documentation native .NET ----------
builder.Services.AddOpenApi();

// Les contrôleurs de l'API sont découverts par le conteneur : les routes sont portées par
// les attributs [Route]/[HttpGet] de chaque contrôleur, sans préfixe de versionnement.
// WithViews, et non AddControllers seul : le filtre de validation antifalsification
// utilisé par [ValidateAntiForgeryToken] appartient à MVC ViewFeatures, que AddControllers
// n'enregistre pas. Sans lui, chaque POST protégé échoue à l'exécution en disant que le
// filtre n'est pas dans le conteneur — l'écran affiche bien, l'enregistrement non.
builder.Services.AddControllersWithViews();

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

// ---------- Configuration de production ----------
// Le refus est délibéré. Une configuration incomplète ne produit aucune erreur visible : le
// site répond, puis la panne n'apparaît qu'au moment où un membre tente de s'inscrire. Mieux
// vaut un démarrage avorté avec la liste de ce qui manque, qu'un site en ligne qui n'envoie
// plus rien. En développement, la même configuration reste permise : un poste local n'a pas
// de serveur de courriel, et c'est un choix, pas une faute.
if (!app.Environment.IsDevelopment())
{
    foreach (var point in ProductionConfiguration.DescribeWarnings(app.Configuration))
    {
        StartupLog.PointDeVigilance(app.Logger, point);
    }

    var problemes = ProductionConfiguration.DescribeBlockingProblems(app.Configuration);

    if (problemes.Count > 0)
    {
        var liste = string.Join(
            Environment.NewLine,
            problemes.Select(probleme => $" - {probleme}"));

        StartupLog.ConfigurationBloquante(app.Logger, liste);

        throw new InvalidOperationException(
            "Configuration de production inexploitable :" + Environment.NewLine + liste);
    }
}

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

// ---------- Rubriques du blog ----------
// Idempotent également : seules les rubriques absentes sont insérées, pour ne pas
// écraser celles qu'un administrateur a pu renommer ou réordonner.
await app.Services.GetRequiredService<RubriqueBootstrapper>().InitializeAsync();

// ---------- Pipeline ----------
// Une seule inscription au middleware d'exception : le premier intercepte toutes les
// exceptions, si bien qu'un second appel resterait sans effet. Les deux branches ci-dessous
// ne sont donc pas cumulables.
// En développement, la réponse ProblemDetails porte la pile d'appels, ce qui aide à diagnostiquer.
// En production, la page « /Error » est Friendly et n'expose aucun détail technique.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// La page « introuvable » est un composant Razor rendu dans la coquille (layout, barre de
// navigation). Posé ici, dans le pipeline principal, le middleware enveloppe tous les
// points de terminaison qui suivent : un chemin sans page produit ainsi une 404 avec le
// corps HTML de la page « introuvable », et non une 404 vide qui ferait perdre la
// navigation à l'utilisateur.
app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

// Garde-fou pour l'API : la ré-exécution ci-dessus rendrait un document HTML pour un 404
// d'API. Posé juste après, ce middleware enveloppe à son tour les points de terminaison :
// pour un chemin d'API, il écrit le corps JSON avant que la ré-exécution n'ait la main
// (le corps commencé empêche la ré-exécution), et un chemin d'API inconnu reste une 404 JSON.
app.Use(async (contexte, suivant) =>
{
    await suivant(contexte);
    if (EstAppelApi(contexte.Request)
        && contexte.Response.StatusCode == StatusCodes.Status404NotFound
        && !contexte.Response.HasStarted)
    {
        contexte.Response.ContentType = "application/problem+json; charset=utf-8";
        await contexte.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            title = "Introuvable",
            status = StatusCodes.Status404NotFound,
            detail = "La ressource demandée n'existe pas."
        });
    }
});

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
app.MapPasswordResetEndpoints();

app.MapNewsletterEndpoints();

app.MapContactEndpoints();

// ---------- API ----------
// Préfixe unique, sans versionnement : le contrat n'a pas encore été publié à des clients
// qu'il serait coûteux de rompre.
app.MapControllers();

// ---------- Santé ----------
app.MapHealthChecks("/health").AllowAnonymous();

// ---------- Documentation ----------
// Restreint au développement : la description OpenAPI énumère l'intégralité des points de
// terminaison, y compris les routes d'administration. Publier cette carte sur un site ouvert
// revient à offrir un annuaire de ce qui existe. Le secreto reste côté serveur ; la surface
// exposée, elle, n'a pas à l'être.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DotnetNiger API");
        options.DocumentTitle = "DotnetNiger API";
    });
}

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


