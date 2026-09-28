using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Suivi des abonnements à la lettre d'information.
/// </summary>
[Route("api/admin/newsletter")]
public sealed class AdminNewsletterController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : ApiControllerBase
{
    /// <summary>
    /// Liste les abonnements.
    /// </summary>
    /// <param name="statut">État à filtrer, ou <see langword="null"/> pour tous.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page d'abonnements.</returns>
    /// <response code="200">Abonnements listés.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">L'appelant n'est pas administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [Route("subscribers")]
    [ProducesResponseType<PageResponse<NewsletterSubscriptionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<NewsletterSubscriptionResponse>>> Lister(
        [FromQuery] NewsletterStatus? statut = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RequireAdministrateur(currentUser);

        var requete = unitOfWork.ReadRepository<NewsletterSubscription>().Query;

        if (statut is not null)
        {
            requete = requete.Where(abonnement => abonnement.Status == statut.Value);
        }

        return await PageOfAsync(
            requete.OrderByDescending(abonnement => abonnement.RequestedAtUtc),
            new PageRequest(page, pageSize),
            ToResponse,
            cancellationToken);
    }

    private static NewsletterSubscriptionResponse ToResponse(NewsletterSubscription abonnement) =>
        new(
            abonnement.Id,
            abonnement.Email,
            abonnement.Status,
            abonnement.RequestedAtUtc,
            abonnement.ConfirmedAtUtc,
            abonnement.UnsubscribedAtUtc);
}

