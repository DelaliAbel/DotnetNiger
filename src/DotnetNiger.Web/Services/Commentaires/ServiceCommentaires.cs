using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Commentaires;

/// <summary>Un commentaire et sa réponse, tels qu'affichés sous une fiche.</summary>
/// <param name="Id">Identifiant du commentaire.</param>
/// <param name="AuteurId">Identifiant de son auteur, pour proposer la suppression.</param>
/// <param name="AuteurName">Nom affiché de l'auteur.</param>
/// <param name="Body">Texte publié.</param>
/// <param name="ParentCommentId">Commentaire auquel il répond, s'il en est un.</param>
/// <param name="CreatedAtUtc">Date de publication.</param>
public sealed record CommentaireVue(
    Guid Id,
    Guid AuteurId,
    string AuteurName,
    string Body,
    Guid? ParentCommentId,
    DateTime CreatedAtUtc);

/// <summary>
/// Lecture et dépôt des commentaires d'un article ou d'un événement.
/// </summary>
/// <remarks>
/// Un commentaire se rattache à l'un des deux Exclusive, jamais aux deux : la contrainte
/// est dans le domaine et dans la base, et ce service ne fait que la respecter. La cible
/// se donne donc sous la forme d'un seul identifiant, celui que la fiche affiche.
/// </remarks>
public sealed class ServiceCommentaires(IUnitOfWork uniteDeTravail)
{
    /// <summary>Commentaires publiés d'une cible, du plus ancien au plus récent.</summary>
    /// <param name="articleId">Article commenté, si applicable.</param>
    /// <param name="eventId">Événement commenté, si applicable.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les commentaires publiés, réponses comprises.</returns>
    public async Task<IReadOnlyList<CommentaireVue>> ListerAsync(
        Guid? articleId,
        Guid? eventId,
        CancellationToken cancellationToken = default)
    {
        // Seuls les commentaires publiés sont visibles. Un commentaire en attente de
        // relecture ou masqué par l'administration ne doit jamais apparaître en public.
        var requete = uniteDeTravail.ReadRepository<Comment>().Query
            .AsNoTracking()
            .Where(comment => comment.Status == CommentStatus.Published);

        requete = articleId is { } article
            ? requete.Where(comment => comment.ArticleId == article)
            : requete.Where(comment => comment.CommunityEventId == eventId);

        var commentaires = await requete
            .OrderBy(comment => comment.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (commentaires.Count == 0)
        {
            return [];
        }

        var auteurs = await NomsDesAuteursAsync(commentaires, cancellationToken);

        return
        [
            .. commentaires.Select(comment => new CommentaireVue(
                comment.Id,
                comment.AuthorId,
                auteurs.TryGetValue(comment.AuthorId, out var nom) ? nom : "Membre",
                comment.Body,
                comment.ParentCommentId,
                comment.CreatedAtUtc)),
        ];
    }

    /// <summary>Dépose un commentaire ou une réponse.</summary>
    /// <param name="articleId">Article commenté, si applicable.</param>
    /// <param name="eventId">Événement commenté, si applicable.</param>
    /// <param name="auteurId">Identifiant de l'auteur.</param>
    /// <param name="texte">Texte du commentaire.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le commentaire déposé, ou le motif du refus.</returns>
    public async Task<(bool Reussi, string Message)> DeposerAsync(
        Guid? articleId,
        Guid? eventId,
        Guid auteurId,
        string texte,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(texte))
        {
            return (false, "Le commentaire est vide.");
        }

        // Le domaine refuse un commentaire sans cible, et interdit de viser les deux à
        // fois : la fiche n'en fournit qu'un.
        if (articleId is not null)
        {
            uniteDeTravail.Repository<Comment>().Add(Comment.ForArticle(articleId.Value, auteurId, texte.Trim()));
        }
        else if (eventId is not null)
        {
            uniteDeTravail.Repository<Comment>().Add(Comment.ForCommunityEvent(eventId.Value, auteurId, texte.Trim()));
        }
        else
        {
            return (false, "Ce commentaire n'est rattaché à rien.");
        }

        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return (true, "Votre commentaire est publié.");
    }

    /// <summary>Supprime un commentaire, à condition d'en être l'auteur.</summary>
    /// <param name="commentaireId">Identifiant du commentaire.</param>
    /// <param name="auteurId">Identifiant du membre connecté.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le résultat de la demande, motif compris en cas de refus.</returns>
    public async Task<(bool Reussi, string Message)> SupprimerAsync(
        Guid commentaireId,
        Guid auteurId,
        CancellationToken cancellationToken = default)
    {
        var commentaire = await uniteDeTravail.ReadRepository<Comment>().Query
            .FirstOrDefaultAsync(candidat => candidat.Id == commentaireId, cancellationToken);

        if (commentaire is null)
        {
            return (false, "Ce commentaire n'existe plus.");
        }

        // Seul l'auteur retire sa parole, ou l'administration par l'API. Le rôle n'est pas
        // vérifié ici : le formulaire est la voie du membre, pas celle de l'administrateur.
        if (commentaire.AuthorId != auteurId)
        {
            return (false, "Vous ne pouvez supprimer que vos propres commentaires.");
        }

        commentaire.MarkDeleted();
        uniteDeTravail.Repository<Comment>().Update(commentaire);
        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return (true, "Votre commentaire est supprimé.");
    }

    /// <summary>
    /// Signale un commentaire à la modération.
    /// </summary>
    /// <param name="commentaireId">Identifiant du commentaire signalé.</param>
    /// <param name="rapporteurId">Identifiant du membre qui signale.</param>
    /// <param name="motif">Motif du signalement.</param>
    /// <param name="details">Précisions facultatives.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le résultat de la demande, motif compris en cas de refus.</returns>
    /// <remarks>
    /// Le signalement ne masque rien et ne suspend rien : il place le commentaire devant la
    /// modération. C'est la seule action qu'un membre autre que l'auteur puisse mener sur un
    /// commentaire, et elle doit rester possible même quand l'auteur est l'administrateur.
    /// </remarks>
    public async Task<(bool Reussi, string Message)> SignalerAsync(
        Guid commentaireId,
        Guid rapporteurId,
        ReportReason motif,
        string? details,
        CancellationToken cancellationToken = default)
    {
        if (!await uniteDeTravail.ReadRepository<Comment>().Query
            .AnyAsync(comment => comment.Id == commentaireId, cancellationToken))
        {
            return (false, "Ce commentaire n'existe plus.");
        }

        // Un même membre ne signale pas deux fois le même commentaire : sinon un spammeur
        // gonflerait la file de modération sans rien y apprendre.
        var dejaSignale = await uniteDeTravail.ReadRepository<Report>().Query
            .AnyAsync(
                signalement => signalement.TargetType == ReportTargetType.Comment
                    && signalement.TargetId == commentaireId
                    && signalement.ReporterId == rapporteurId
                    && signalement.Status != ReportStatus.Resolved,
                cancellationToken);

        if (dejaSignale)
        {
            return (false, "Vous avez déjà signalé ce commentaire.");
        }

        uniteDeTravail.Repository<Report>().Add(
            new Report(ReportTargetType.Comment, commentaireId, motif, rapporteurId, details));

        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        return (true, "Merci : la modération va examiner ce commentaire.");
    }

    /// <summary>
    /// Noms des auteurs en une seule requête, pour ne pas déclencher une requête par
    /// commentaire affiché.
    /// </summary>
    private async Task<Dictionary<Guid, string>> NomsDesAuteursAsync(
        IReadOnlyCollection<Comment> commentaires,
        CancellationToken cancellationToken)
    {
        var identifiants = commentaires.Select(comment => comment.AuthorId).Distinct().ToArray();

        if (identifiants.Length == 0)
        {
            return [];
        }

        return await uniteDeTravail.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .Where(membre => identifiants.Contains(membre.Id))
            .ToDictionaryAsync(membre => membre.Id, membre => membre.FullName, cancellationToken);
    }
}
