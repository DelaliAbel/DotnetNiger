using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Suppression définitive d'un compte et des contenus qu'il a produits.
/// </summary>
/// <remarks>
/// Les clés étrangères qui relient les contenus à un compte interdisent la suppression
/// (comportement <c>Restrict</c>) : une exception à la règle générale, choisie pour
/// qu'un contenu ne disparaisse jamais silencieusement quand un compte est effacé. Cette
/// suppression est donc une exception explicite à cette règle, et elle retire les
/// dépendances dans l'ordre qu'exigent ces contraintes.
/// <para>
/// Les suppressions utilisent <c>ExecuteDeleteAsync</c>, et c'est indispensable :
/// l'intercepteur d'audit transforme toute suppression physique suivie d'un
/// <c>SaveChanges</c> en retrait logique, et le filtre global des entités supprimables
/// masquerait en plus les lignes déjà retirées par leur auteur, qui continueraient sinon
/// de bloquer la suppression du compte. Une suppression demandée par un état suivi échouerait
/// donc toujours, en laissant une partie du contenu en place. <c>ExecuteDeleteAsync</c>
/// contourne le suivi et l'intercepteur, et émet réellement les <c>DELETE</c>.
/// </para>
/// </remarks>
public sealed partial class AccountDeletionService(
    DotnetNigerDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<AccountDeletionService> logger) : IAccountDeletionService
{
    /// <inheritdoc />
    public async Task<AccountDeletionReport?> DeleteAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        var account = await userManager.FindByIdAsync(accountId.ToString());

        if (account is null)
        {
            return null;
        }

        var email = account.Email;

        // Une seule transaction : à mi-parcours, un échec laisserait un compte amputé de
        // ses contenus mais toujours actif, sans aucun moyen de savoir ce qui a disparu.
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Le rendu des signalements traités passe par le suivi EF, car ils survivent au
        // compte : ils sont enregistrés avant tout le reste, tant que le suivi est encore
        // disponible.
        var released = await LibererLesSignalementsTraitesAsync(accountId, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Le suivi est alors abandonné, et c'est ce qui rend la suite possible. Les
        // suppressions ci-dessous émettent des <c>DELETE</c> directs que le suivi ignore,
        // et il referait ensuite réinscrire ou mettre à jour des lignes disparues. surtout,
        // <c>UserManager.DeleteAsync</c> marque le compte comme supprimé et EF cherchera
        // alors à détacher les dépendances obligatoires encore suivies : toutes les
        // relations du domaine étant en <c>Restrict</c>, il échouerait sur la première,
        // sans même envoyer la requête.
        context.ChangeTracker.Clear();

        // Les commentaires d'abord : plusieurs d'entre eux peuvent être les parents
        // d'autres, et les retirer avant les articles évite de laisser des réponses
        // pointant vers un auteur déjà effacé. Les inscriptions précèdent leur événement,
        // pour que la base ne les supprime pas en cascade avant d'avoir été comptées.
        var comments = await SupprimerLesCommentairesAsync(accountId, cancellationToken);
        var registrations = await SupprimerLesInscriptionsAsync(accountId, cancellationToken);
        var articles = await SupprimerLesArticlesAsync(accountId, cancellationToken);
        var resources = await SupprimerLesRessourcesAsync(accountId, cancellationToken);
        var projects = await SupprimerLesProjetsAsync(accountId, cancellationToken);
        var events = await SupprimerLesEvenementsAsync(accountId, cancellationToken);
        var reports = await SupprimerLesSignalementsAsync(accountId, cancellationToken);

        // Rôles, demandes de confirmation, jetons et connexions externes tiennent tous au
        // compte : le magasin d'Identity les retire avec lui.
        var result = await userManager.DeleteAsync(account);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Le compte {accountId} n'a pas pu être supprimé : "
                + string.Join(" ", result.Errors.Select(erreur => erreur.Description)));
        }

        await transaction.CommitAsync(cancellationToken);

        var bilan = new AccountDeletionReport(
            comments, articles, resources, projects, events, registrations, reports, released);

        CompteSupprime(logger, accountId, email, bilan);

        return bilan;
    }

    private async Task<int> SupprimerLesCommentairesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var comments = context.Comments
            .IgnoreQueryFilters()
            .Where(comment => comment.AuthorId == accountId);

        return await comments.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> SupprimerLesArticlesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var articles = context.Articles
            .IgnoreQueryFilters()
            .Where(article => article.AuthorId == accountId);

        return await articles.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> SupprimerLesRessourcesAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var ressources = context.Resources
            .IgnoreQueryFilters()
            .Where(ressource => ressource.CreatorId == accountId);

        return await ressources.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> SupprimerLesProjetsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var projets = context.Projects
            .IgnoreQueryFilters()
            .Where(projet => projet.CreatedById == accountId);

        return await projets.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> SupprimerLesEvenementsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var evenements = context.CommunityEvents
            .IgnoreQueryFilters()
            .Where(communityEvent => communityEvent.CreatedById == accountId);

        return await evenements.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> SupprimerLesInscriptionsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var inscriptions = context.EventRegistrations
            .Where(inscription => inscription.MemberId == accountId);

        return await inscriptions.ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<int> SupprimerLesSignalementsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var signalements = context.Reports.Where(report => report.ReporterId == accountId);

        return await signalements.ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Rend à la modération les signalements que le compte traitait.
    /// </summary>
    /// <remarks>
    /// Le signalement n'appartient pas au compte : c'est une décision de modération
    /// prise au nom de la communauté. L'effacer avec le compte perdrait cette décision
    /// et son horodatage, alors que seule la référence au gestionnaire est devenue obsolète.
    /// </remarks>
    private async Task<int> LibererLesSignalementsTraitesAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var traites = await context.Reports
            .Where(report => report.HandledById == accountId)
            .ToListAsync(cancellationToken);

        foreach (var report in traites)
        {
            report.ReleaseHandler();
        }

        return traites.Count;
    }

    [LoggerMessage(
        EventId = 40,
        Level = LogLevel.Information,
        Message = "Compte {AccountId} ({Email}) supprimé définitivement. Bilan : {Report}.")]
    private static partial void CompteSupprime(
        ILogger logger,
        Guid accountId,
        string? email,
        AccountDeletionReport report);
}
