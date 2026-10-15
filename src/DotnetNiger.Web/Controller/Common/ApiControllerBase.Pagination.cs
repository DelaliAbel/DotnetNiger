using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller.Common;

/// <summary>
/// Pagination par entité, avec résolution des noms d'auteur en une requête.
/// </summary>
/// <remarks>
/// Chaque méthode compte, page, puis résout les noms : jamais une requête par élément, sinon
/// une page de vingt articles produirait vingt allers-retours.
/// </remarks>
public abstract partial class ApiControllerBase
{
    /// <summary>
    /// Exécute une requête paginée d'articles en résolvant les noms d'auteur en une requête.
    /// </summary>
    /// <param name="requete">Requête filtrée et triée.</param>
    /// <param name="page">Demande de pagination.</param>
    /// <param name="unitOfWork">Unité de travail.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de résumés d'articles.</returns>
    protected static async Task<ActionResult<PageResponse<ArticleSummaryResponse>>> ArticlePageOfAsync(
        IQueryable<Article> requete,
        PageRequest page,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) =>
        await ArticlePageOfAsync(requete, page, unitOfWork, ApiProjections.ToSummary, cancellationToken);

    /// <summary>
    /// Exécute une requête paginée d'articles en résolvant les noms d'auteur en une requête.
    /// </summary>
    /// <typeparam name="TResult">Type de la réponse.</typeparam>
    /// <param name="requete">Requête filtrée et triée.</param>
    /// <param name="page">Demande de pagination.</param>
    /// <param name="unitOfWork">Unité de travail.</param>
    /// <param name="projection">Projection recevant l'article et le nom de son auteur.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de réponses.</returns>
    protected static async Task<ActionResult<PageResponse<TResult>>> ArticlePageOfAsync<TResult>(
        IQueryable<Article> requete,
        PageRequest page,
        IUnitOfWork unitOfWork,
        Func<Article, string, TResult> projection,
        CancellationToken cancellationToken)
    {
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<Article>(page),
                article => projection(article, string.Empty));
        }

        var items = await requete
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(article => article.AuthorId).Distinct().ToArray();
        var noms = await NomsAsync(unitOfWork, ids, cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<Article>(items, page.PageNumber, page.PageSize, total),
            article => projection(article, noms.GetValueOrDefault(article.AuthorId, string.Empty)));
    }

    /// <summary>
    /// Exécute une requête paginée de ressources en résolvant les noms de créateur en une requête.
    /// </summary>
    /// <typeparam name="TResult">Type de la réponse.</typeparam>
    /// <param name="requete">Requête filtrée et triée.</param>
    /// <param name="page">Demande de pagination.</param>
    /// <param name="unitOfWork">Unité de travail.</param>
    /// <param name="projection">Projection recevant la ressource et le nom de son créateur.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de réponses.</returns>
    protected static async Task<ActionResult<PageResponse<TResult>>> ResourcePageOfAsync<TResult>(
        IQueryable<Resource> requete,
        PageRequest page,
        IUnitOfWork unitOfWork,
        Func<Resource, string, TResult> projection,
        CancellationToken cancellationToken)
    {
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<Resource>(page),
                resource => projection(resource, string.Empty));
        }

        var items = await requete
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(resource => resource.CreatorId).Distinct().ToArray();
        var noms = await NomsAsync(unitOfWork, ids, cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<Resource>(items, page.PageNumber, page.PageSize, total),
            resource => projection(resource, noms.GetValueOrDefault(resource.CreatorId, string.Empty)));
    }

    /// <summary>
    /// Exécute une requête paginée de projets en résolvant les noms de créateur en une requête.
    /// </summary>
    /// <typeparam name="TResult">Type de la réponse.</typeparam>
    /// <param name="requete">Requête filtrée et triée.</param>
    /// <param name="page">Demande de pagination.</param>
    /// <param name="unitOfWork">Unité de travail.</param>
    /// <param name="projection">Projection recevant le projet et le nom de son créateur.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de réponses.</returns>
    protected static async Task<ActionResult<PageResponse<TResult>>> ProjectPageOfAsync<TResult>(
        IQueryable<Project> requete,
        PageRequest page,
        IUnitOfWork unitOfWork,
        Func<Project, string, TResult> projection,
        CancellationToken cancellationToken)
    {
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<Project>(page),
                project => projection(project, string.Empty));
        }

        var items = await requete
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(project => project.CreatedById).Distinct().ToArray();
        var noms = await NomsAsync(unitOfWork, ids, cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<Project>(items, page.PageNumber, page.PageSize, total),
            project => projection(project, noms.GetValueOrDefault(project.CreatedById, string.Empty)));
    }

    /// <summary>
    /// Exécute une requête paginée d'événements en résolvant les noms de créateur et les
    /// inscriptions confirmées en une requête chacun.
    /// </summary>
    /// <typeparam name="TResult">Type de la réponse.</typeparam>
    /// <param name="requete">Requête filtrée et triée.</param>
    /// <param name="page">Demande de pagination.</param>
    /// <param name="unitOfWork">Unité de travail.</param>
    /// <param name="projection">Projection recevant l'événement, le nombre de confirmés et le nom du créateur.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de réponses.</returns>
    protected static async Task<ActionResult<PageResponse<TResult>>> EventPageOfAsync<TResult>(
        IQueryable<CommunityEvent> requete,
        PageRequest page,
        IUnitOfWork unitOfWork,
        Func<CommunityEvent, int, string, TResult> projection,
        CancellationToken cancellationToken)
    {
        var total = await requete.CountAsync(cancellationToken);

        if (total == 0)
        {
            return ApiProjections.ToPage(
                PagedResultFactory.Empty<CommunityEvent>(page),
                communityEvent => projection(communityEvent, 0, string.Empty));
        }

        var items = await requete
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        var ids = items.Select(communityEvent => communityEvent.CreatedById).Distinct().ToArray();
        var noms = await NomsAsync(unitOfWork, ids, cancellationToken);

        var confirmes = await unitOfWork.ReadRepository<EventRegistration>().Query
            .Where(inscription => inscription.Status == EventRegistrationStatus.Confirmed)
            .Where(inscription => items.Select(communityEvent => communityEvent.Id)
                .Contains(inscription.CommunityEventId))
            .GroupBy(inscription => inscription.CommunityEventId)
            .Select(groupe => new { Id = groupe.Key, Total = groupe.Count() })
            .ToDictionaryAsync(resultat => resultat.Id, resultat => resultat.Total, cancellationToken);

        return ApiProjections.ToPage(
            new PagedResult<CommunityEvent>(items, page.PageNumber, page.PageSize, total),
            communityEvent => projection(
                communityEvent,
                confirmes.GetValueOrDefault(communityEvent.Id),
                noms.GetValueOrDefault(communityEvent.CreatedById, string.Empty)));
    }
}