using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Infrastructure.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Abonnement à la lettre d'information depuis le formulaire du pied de page.
/// </summary>
/// <remarks>
/// L'API <c>POST /api/newsletter/subscriptions</c> attend du JSON : un formulaire HTML ne peut
/// pas en poster sans JavaScript, et le pied de page ne mérite pas d'hydrater Blazor pour
/// une inscription. Ce point de terminaison accepte donc le formulaire et renvoie la même
/// information que l'API, y compris son rythme : la confirmation part toujours, que
/// l'adresse soit nouvelle, déjà confirmée ou en attente.
/// </remarks>
public static class NewsletterEndpoints
{
    /// <summary>
    /// Déclare l'abonnement posté par le formulaire du pied de page.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    public static WebApplication MapNewsletterEndpoints(this WebApplication app)
    {
        // Un formulaire ne se soumet qu'une fois, mais rien n'interdit de rejouer la requête.
        // Le compteur de limitation de débit, posé en amont dans le pipeline, borne ce cas
        // comme tous les autres envois répétés.
        app.MapPost("/infolettre/sabonner", async (
            [FromForm] SubscribeInput input,
            IUnitOfWork unitOfWork,
            IEmailDispatcher emails,
            IOptions<EmailOptions> emailOptions,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var adresse = input.Email?.Trim() ?? string.Empty;

            if (!adresse.Contains('@', StringComparison.Ordinal) || adresse.Length > 320)
            {
                return Results.Redirect(
                    $"{CheminDeRetour(input.Retour)}?erreur={Uri.EscapeDataString("Adresse e-mail invalide.")}");
            }

            var maintenant = DateTime.UtcNow;
            var (jeton, empreinte) = SubscriptionToken.Creer();
            var cleDesabonnement = SubscriptionToken.CreerCleDesabonnement();

            var abonnement = await unitOfWork.ReadRepository<NewsletterSubscription>().Query
                .FirstOrDefaultAsync(candidat => candidat.Email == adresse, cancellationToken);

            if (abonnement is null)
            {
                abonnement = NewsletterSubscription.Create(adresse, empreinte, cleDesabonnement, maintenant);
                unitOfWork.Repository<NewsletterSubscription>().Add(abonnement);
            }
            else if (abonnement.IsConfirmed)
            {
                // Déjà abonnée : pas de second lien d'envoi, ce serait une façon de tester qui
                // l'est. Le message reste néanmoins identique, sinon la redirection elle-même
                // dirait si l'adresse est dans la liste.
                NewsletterLog.DemandeIgnoree(logger);

                return Results.Redirect(NewsletterRedirect.Confirmation(adresse, input.Retour));
            }
            else
            {
                // Abonnement en attente de confirmation : le jeton est renouvelé pour qu'un
                // courriel perdu reste rattrapable.
                abonnement.RequestAgain(empreinte, maintenant);
                unitOfWork.Repository<NewsletterSubscription>().Update(abonnement);
            }

            await emails.DispatchAsync(
                EmailTemplateKeys.NewsletterConfirmation,
                adresse,
                EmailValues.De(
                    (EmailVariables.UrlDesabonnement, emailOptions.Value.BuildUrl(
                        $"/api/newsletter/subscriptions/confirm?token={Uri.EscapeDataString(jeton)}"))),
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            NewsletterLog.DemandeEnregistree(logger);

            return Results.Redirect(NewsletterRedirect.Confirmation(adresse, input.Retour));
        });

        return app;
    }

    /// <summary>Adresse et page de retour transmises par le formulaire.</summary>
    /// <param name="Email">Adresse à abonner.</param>
    /// <param name="Retour">Page depuis laquelle le formulaire a été soumis.</param>
    public sealed record SubscribeInput(string? Email, string? Retour);

    /// <summary>
    /// Réduit la valeur du champ <c>Retour</c> à l'un des chemins du site public.
    /// </summary>
    /// <remarks>
    /// Le pied de page apparaît sur toutes les pages : le message doit s'afficher sous les
    /// yeux de la personne qui vient de s'abonner. Mais le chemin vient du formulaire, donc
    /// de l'extérieur : sans cette réduction, il permettrait de faire pointer la redirection
    /// vers un autre site. Réduire une URL absolue à sa seule partie chemin ne suffit pas —
    /// « https://ailleurs.example/vol » deviendrait «/vol», une page de notre site sans
    /// rapport avec ce qui a été demandé. On ne conserve donc que les pages où le pied de
    /// page propose réellement l'inscription.
    /// </remarks>
    /// <param name="retour">Valeur transmise par le formulaire.</param>
    /// <returns>Un chemin de la liste blanche, par défaut la page d'accueil.</returns>
    internal static string CheminDeRetour(string? retour)
    {
        if (string.IsNullOrWhiteSpace(retour))
        {
            return "/";
        }

        var chemin = retour.Trim();

        var indexInterrogation = chemin.IndexOf('?', StringComparison.Ordinal);
        if (indexInterrogation >= 0)
        {
            chemin = chemin[..indexInterrogation];
        }

        return CheminsAutorises.Contains(chemin) ? chemin : "/";
    }

    /// <summary>Pages sur lesquelles la redirection après abonnement est acceptée.</summary>
    private static readonly HashSet<string> CheminsAutorises = new(StringComparer.Ordinal)
    {
        "/",
        "/a-propos",
        "/ressources",
        "/communaute",
        "/blog",
        "/evenements",
        "/partenaires",
        "/contact",
    };
}

/// <summary>
/// Destinations de redirection du formulaire d'abonnement, et leur message commun.
/// </summary>
internal static class NewsletterRedirect
{
    /// <summary>
    /// Message affiché après l'envoi, identique pour toute adresse.
    /// </summary>
    /// <param name="email">Adresse saisie, rappelée pour que la personne vérifie sa saisie.</param>
    /// <param name="retour">Page depuis laquelle le formulaire a été soumis.</param>
    /// <returns>URL de redirection vers cette page, porteuse du message.</returns>
    public static string Confirmation(string email, string? retour) =>
        $"{NewsletterEndpoints.CheminDeRetour(retour)}?confirme={Uri.EscapeDataString(email)}";
}

/// <summary>Journalisation des demandes d'abonnement.</summary>
internal static partial class NewsletterLog
{
    // L'adresse n'est pas écrite dans le journal : c'est une donnée personnelle, et
    // elle n'apporte rien ici puisque la demande est comptée.
    [LoggerMessage(EventId = 140, Level = LogLevel.Information,
        Message = "Demande d'abonnement a la lettre enregistree")]
    public static partial void DemandeEnregistree(ILogger logger);

    [LoggerMessage(EventId = 141, Level = LogLevel.Debug,
        Message = "Demande d'abonnement ignoree pour une adresse deja confirmee")]
    public static partial void DemandeIgnoree(ILogger logger);
}
