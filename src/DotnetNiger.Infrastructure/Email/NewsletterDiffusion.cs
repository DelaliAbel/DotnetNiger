using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Envoie la lettre d'information aux abonnés confirmés.
/// </summary>
/// <remarks>
/// La publication d'un événement, d'un article, d'une ressource ou d'un projet déclenche
/// l'envoi sans relecture : c'est ce que l'association a demandé, et c'est cohérent avec le
/// fait que la publication elle-même est déjà un acte volontaire d'un administrateur.
/// <para>
/// Un message par destinataire, jamais une liste en copie cachée. La clé de désabonnement
/// étant propre à chaque abonné, une seule adresse doit apparaître dans chaque envoi : c'est
/// la condition pour que le lien de désinscription reste le sien.
/// </para>
/// </remarks>
public interface INewsletterDiffusion
{
    /// <summary>
    /// Met en file la lettre annonçant une publication, pour chaque abonné confirmé.
    /// </summary>
    /// <param name="publication">Contenu à annoncer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le nombre de messages effectivement mis en file.</returns>
    Task<int> DiffuserAsync(NouvellePublication publication, CancellationToken cancellationToken);
}

/// <summary>
/// Contenu publié à annoncer dans la lettre.
/// </summary>
/// <param name="Type">Nature du contenu, en français, telle qu'elle apparaît dans la lettre.</param>
/// <param name="Titre">Titre du contenu.</param>
/// <param name="Resume">Phrase d'accroche, en texte brut.</param>
/// <param name="Chemin">Adresse du contenu sur le site, sans le nom de domaine.</param>
public sealed record NouvellePublication(string Type, string Titre, string Resume, string Chemin)
{
    /// <summary>
    /// Annonce un article de blog.
    /// </summary>
    /// <param name="article">Article publié.</param>
    /// <returns>Publication à annoncer.</returns>
    public static NouvellePublication De(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        return new NouvellePublication("Article", article.Title, article.Excerpt, $"/articles/{article.Slug}");
    }

    /// <summary>
    /// Annonce un événement.
    /// </summary>
    /// <param name="communityEvent">Événement publié.</param>
    /// <returns>Publication à annoncer.</returns>
    public static NouvellePublication De(CommunityEvent communityEvent)
    {
        ArgumentNullException.ThrowIfNull(communityEvent);

        return new NouvellePublication(
            "Événement",
            communityEvent.Title,
            communityEvent.Summary,
            $"/evenements/{communityEvent.Slug}");
    }

    /// <summary>
    /// Annonce une ressource.
    /// </summary>
    /// <param name="resource">Ressource publiée.</param>
    /// <returns>Publication à annoncer.</returns>
    public static NouvellePublication De(Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        // Une ressource n'a pas d'extrait : sa description est rédigée en HTML, qu'un message
        // ne peut pas afficher. Le texte est donc réduit à son contenu lisible.
        return new NouvellePublication(
            "Ressource",
            resource.Title,
            TexteLisible.De(resource.Description),
            $"/ressources/{resource.Slug}");
    }

    /// <summary>
    /// Annonce un projet.
    /// </summary>
    /// <param name="project">Projet publié.</param>
    /// <returns>Publication à annoncer.</returns>
    public static NouvellePublication De(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        return new NouvellePublication(
            "Projet",
            project.Name,
            project.Summary,
            $"/projets/{project.Slug}");
    }
}

/// <inheritdoc cref="INewsletterDiffusion" />
public sealed partial class NewsletterDiffusion(
    IUnitOfWork unitOfWork,
    IEmailDispatcher emails,
    IOptions<EmailOptions> emailOptions,
    ILogger<NewsletterDiffusion> logger) : INewsletterDiffusion
{
    /// <inheritdoc />
    public async Task<int> DiffuserAsync(NouvellePublication publication, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(publication);

        // Seuls les abonnements confirmés reçoivent la lettre : une demande encore en attente
        // de confirmation est une adresse qui n'a pas encore dit oui.
        var abonnes = await unitOfWork.ReadRepository<NewsletterSubscription>().Query
            .Where(abonnement => abonnement.Status == NewsletterStatus.Confirmed)
            .OrderBy(abonnement => abonnement.ConfirmedAtUtc)
            .Select(abonnement => new { abonnement.Email, abonnement.UnsubscribeKey })
            .ToListAsync(cancellationToken);

        if (abonnes.Count == 0)
        {
            DiffusionSansAbonne(logger);
            return 0;
        }

        var urlContenu = emailOptions.Value.BuildUrl(publication.Chemin);
        var urlDesabonnement = emailOptions.Value.BuildUrl("/api/newsletter/unsubscribe");
        var misesEnFile = 0;

        foreach (var abonne in abonnes)
        {
            if (await emails.DispatchAsync(
                EmailTemplateKeys.NewsletterNewContent,
                abonne.Email,
                EmailValues.De(
                    (EmailVariables.TypeContenu, publication.Type),
                    (EmailVariables.TitreContenu, publication.Titre),
                    (EmailVariables.ResumeContenu, publication.Resume),
                    (EmailVariables.UrlContenu, urlContenu),
                    (
                        EmailVariables.UrlDesabonnement,
                        $"{urlDesabonnement}?key={Uri.EscapeDataString(abonne.UnsubscribeKey)}")),
                cancellationToken))
            {
                misesEnFile++;
            }
        }

        // La file d'attente vit dans la transaction de l'appelant : sans cet enregistrement,
        // les messages seraient perdus. La publication, elle, est déjà commise — un échec
        // ici n'a donc pas à défaire ce qui est en ligne.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        DiffusionMiseEnFile(logger, publication.Type, publication.Titre, misesEnFile);

        return misesEnFile;
    }

    [LoggerMessage(
        EventId = 130,
        Level = LogLevel.Information,
        Message = "Lettre mise en file pour {Nombre} abonnes : {Type} « {Titre} »")]
    private static partial void DiffusionMiseEnFile(ILogger logger, string type, string titre, int nombre);

    [LoggerMessage(
        EventId = 131,
        Level = LogLevel.Information,
        Message = "Publication annoncee dans la lettre, sans abonne confirme")]
    private static partial void DiffusionSansAbonne(ILogger logger);
}
