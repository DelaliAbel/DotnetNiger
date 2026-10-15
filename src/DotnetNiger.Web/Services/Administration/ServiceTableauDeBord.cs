using System.Globalization;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Identity;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un contenu récent, tel que l'affiche le tableau de bord.</summary>
/// <param name="Titre">Titre du contenu.</param>
/// <param name="Adresse">Adresse de la fiche publique.</param>
/// <param name="Meta">Précision contextuelle : date affichée, ou mention de publication.</param>
/// <param name="Etat">État de publication, pour la couleur de l'étiquette.</param>
public sealed record ContenuRecent(
    string Titre,
    string Adresse,
    string Meta,
    PublicationStatus Etat);

/// <summary>
/// Compteurs et derniers contenus du tableau de bord d'administration.
/// </summary>
/// <remarks>
/// Une tâche mémoïsée par service : la barre latérale et la page lisent les mêmes
/// chiffres. Sur un même contexte de base, deux lectures parallèles sont refusées par EF ;
/// c'est ce qui faisait tomber l'administration en erreur 500.
/// <para>
/// Les lectures restent séquentielles : elles partagent le contexte de l'appelant, et les
/// lancer en parallèle reproduirait exactement l'erreur que la tâche partagée évite.
/// </para>
/// </remarks>
public sealed class ServiceTableauDeBord(IUnitOfWork uniteDeTravail)
{
    /// <summary>Culture des dates affichées : le site parle français, quelle que soit la machine.</summary>
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("fr-FR");

    private Task<CompteursVue>? _lectureCompteurs;

    private Task<TableauDeBordVue>? _lecture;

    /// <summary>
    /// Lit les seuls compteurs, pour la barre latérale.
    /// </summary>
    /// <remarks>
    /// La barre a besoin de quatre chiffres, pas des quinze derniers contenus : les lire
    /// quand même ferait payer à chaque écran d'administration trois requêtes de plus, dont
    /// des tables entières chargées en mémoire pour cinq lignes affichées.
    /// </remarks>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les compteurs affichés en regard des entrées de la barre.</returns>
    public Task<CompteursVue> LireCompteursAsync(CancellationToken cancellationToken = default)
        => (_lectureCompteurs ??= LireCompteursInterneAsync(cancellationToken));

    /// <summary>
    /// Lit l'ensemble du tableau de bord.
    /// </summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Compteurs et derniers contenus.</returns>
    public Task<TableauDeBordVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Relit tout, après une action d'administration.</summary>
    public void Oublie()
    {
        _lecture = null;
        _lectureCompteurs = null;
    }

    private async Task<CompteursVue> LireCompteursInterneAsync(CancellationToken cancellationToken)
    {
        var commentaires = await uniteDeTravail.ReadRepository<Comment>().Query
            .CountAsync(commentaire => commentaire.Status == CommentStatus.PendingReview, cancellationToken);

        var signalements = await uniteDeTravail.ReadRepository<Report>().Query
            .CountAsync(signalement => signalement.Status == ReportStatus.Open
                || signalement.Status == ReportStatus.UnderReview, cancellationToken);

        var messages = await uniteDeTravail.ReadRepository<ContactMessage>().Query
            .CountAsync(message => message.HandledAtUtc == null, cancellationToken);

        var brouillons = await uniteDeTravail.ReadRepository<Article>().Query
            .CountAsync(article => article.Status == PublicationStatus.Draft, cancellationToken);

        return new CompteursVue(commentaires, signalements, messages, brouillons);
    }

    private async Task<TableauDeBordVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var compteurs = await LireCompteursAsync(cancellationToken);

        var maintenant = DateTime.UtcNow;

        var membres = await uniteDeTravail.ReadRepository<ApplicationUser>().Query
            .CountAsync(compte => compte.IsActive, cancellationToken);

        var articlesPublies = await uniteDeTravail.ReadRepository<Article>().Query
            .CountAsync(article => article.Status == PublicationStatus.Published, cancellationToken);

        var ressources = await uniteDeTravail.ReadRepository<Resource>().Query
            .CountAsync(ressource => ressource.Status == PublicationStatus.Published, cancellationToken);

        var evenements = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .CountAsync(evenement => evenement.Status == PublicationStatus.Published, cancellationToken);

        var evenementsAVenir = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .CountAsync(evenement => evenement.Status == PublicationStatus.Published
                && evenement.StartsAtUtc >= maintenant, cancellationToken);

        var projets = await uniteDeTravail.ReadRepository<Project>().Query
            .CountAsync(projet => projet.Status == PublicationStatus.Published, cancellationToken);

        var partenaires = await uniteDeTravail.ReadRepository<Partner>().Query
            .CountAsync(partenaire => partenaire.IsActive, cancellationToken);

        var abonnes = await uniteDeTravail.ReadRepository<NewsletterSubscription>().Query
            .CountAsync(abonnement => abonnement.Status == NewsletterStatus.Confirmed, cancellationToken);

        var derniersArticles = await LireArticlesRecentsAsync(cancellationToken);

        var derniersEvenements = await LireEvenementsRecentsAsync(cancellationToken);

        var dernieresRessources = await LireRessourcesRecentesAsync(cancellationToken);

        return new TableauDeBordVue(
            membres,
            articlesPublies,
            compteurs.Brouillons,
            ressources,
            evenements,
            evenementsAVenir,
            projets,
            partenaires,
            compteurs.Comments,
            compteurs.Signalements,
            compteurs.Messages,
            abonnes,
            derniersArticles,
            derniersEvenements,
            dernieresRessources);
    }

    /// <summary>
    /// Les dates sont triées et récupérées par la base, puis mises en forme en mémoire.
    /// </summary>
    /// <remarks>
    /// Le formatage se fait après la requête, jamais dans la projection : un
    /// <c>ToString</c> avec un modèle de date n'est pas une opération que le fournisseur
    /// SQL sait traduire, et l'approbation de commande échouerait à l'exécution.
    /// </remarks>
    private async Task<IReadOnlyList<ContenuRecent>> LireArticlesRecentsAsync(CancellationToken cancellationToken)
    {
        var lignes = await uniteDeTravail.ReadRepository<Article>().Query
            .AsNoTracking()
            .OrderByDescending(article => article.PublishedAtUtc ?? article.CreatedAtUtc)
            .Take(5)
            .Select(article => new { article.Title, article.Slug, article.PublishedAtUtc, article.Status })
            .ToListAsync(cancellationToken);

        return [.. lignes.Select(ligne => new ContenuRecent(
            ligne.Title,
            "/blog/" + ligne.Slug,
            ligne.PublishedAtUtc.HasValue
                ? ligne.PublishedAtUtc.Value.ToString("d MMMM yyyy", Culture)
                : "non publié",
            ligne.Status))];
    }

    private async Task<IReadOnlyList<ContenuRecent>> LireEvenementsRecentsAsync(CancellationToken cancellationToken)
    {
        var lignes = await uniteDeTravail.ReadRepository<CommunityEvent>().Query
            .AsNoTracking()
            .OrderBy(evenement => evenement.StartsAtUtc)
            .Take(5)
            .Select(evenement => new { evenement.Title, evenement.Slug, evenement.StartsAtUtc, evenement.Status })
            .ToListAsync(cancellationToken);

        return [.. lignes.Select(ligne => new ContenuRecent(
            ligne.Title,
            "/evenements/" + ligne.Slug,
            ligne.StartsAtUtc.ToString("d MMMM yyyy", Culture),
            ligne.Status))];
    }

    private async Task<IReadOnlyList<ContenuRecent>> LireRessourcesRecentesAsync(CancellationToken cancellationToken)
    {
        var lignes = await uniteDeTravail.ReadRepository<Resource>().Query
            .AsNoTracking()
            .OrderByDescending(ressource => ressource.PublishedAtUtc ?? ressource.CreatedAtUtc)
            .Take(5)
            .Select(ressource => new { ressource.Title, ressource.Slug, ressource.PublishedAtUtc, ressource.Status })
            .ToListAsync(cancellationToken);

        return [.. lignes.Select(ligne => new ContenuRecent(
            ligne.Title,
            "/ressources/" + ligne.Slug,
            ligne.PublishedAtUtc.HasValue
                ? ligne.PublishedAtUtc.Value.ToString("d MMMM yyyy", Culture)
                : "non publiée",
            ligne.Status))];
    }
}

/// <summary>Compteurs affichés en regard des entrées de la barre latérale.</summary>
/// <param name="Comments">Commentaires en attente de réexamen.</param>
/// <param name="Signalements">Signalements ouverts ou en cours de traitement.</param>
/// <param name="Messages">Messages de contact sans réponse.</param>
/// <param name="Brouillons">Articles rédigés et non publiés.</param>
public sealed record CompteursVue(
    int Comments,
    int Signalements,
    int Messages,
    int Brouillons);

/// <summary>Compteurs et derniers contenus lus pour le tableau de bord.</summary>
/// <param name="Membres">Comptes de membres actifs.</param>
/// <param name="ArticlesPublies">Articles du blog en ligne.</param>
/// <param name="Brouillons">Articles rédigés et non publiés.</param>
/// <param name="Ressources">Ressources en ligne.</param>
/// <param name="Evenements">Événements publiés.</param>
/// <param name="EvenementsAVenir">Événements publiés et pas encore commencés.</param>
/// <param name="Projets">Projets publiés.</param>
/// <param name="Partenaires">Partenaires actifs.</param>
/// <param name="Commentaires">Commentaires en attente de réexamen.</param>
/// <param name="Signalements">Signalements ouverts ou en cours de traitement.</param>
/// <param name="Messages">Messages de contact sans réponse.</param>
/// <param name="Abonnes">Abonnés recevant la lettre d'information.</param>
/// <param name="DerniersArticles">Derniers articles modifiés ou publiés.</param>
/// <param name="DerniersEvenements">Événements les plus proches.</param>
/// <param name="DernieresRessources">Dernières ressources déposées.</param>
public sealed record TableauDeBordVue(
    int Membres,
    int ArticlesPublies,
    int Brouillons,
    int Ressources,
    int Evenements,
    int EvenementsAVenir,
    int Projets,
    int Partenaires,
    int Commentaires,
    int Signalements,
    int Messages,
    int Abonnes,
    IReadOnlyList<ContenuRecent> DerniersArticles,
    IReadOnlyList<ContenuRecent> DerniersEvenements,
    IReadOnlyList<ContenuRecent> DernieresRessources)
{
    /// <summary>
    /// Ce qui attend une décision : sans cela, le tableau de bord ne fait que dire
    /// combien il y a de contenus, ce que chaque liste sait déjà.
    /// </summary>
    public int ATraiter => Commentaires + Signalements + Messages;
}