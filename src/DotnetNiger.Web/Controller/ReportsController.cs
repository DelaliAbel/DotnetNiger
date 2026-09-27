using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Common;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Controller;

/// <summary>
/// Signalement de contenu et modération.
/// </summary>
/// <remarks>
/// Un signalement est déposé par un membre à propos d'un contenu, puis traité par un
/// administrateur. Les deux rôles sont distincts : celui qui signale ne doit pas pouvoir
/// clore son propre signalement, sinon la modération n'apporterait rien.
/// </remarks>
[Route("api/reports")]
[Tags("Signalements")]
public sealed class ReportsController(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    TimeProvider clock) : ApiControllerBase
{
    /// <summary>Signale un contenu comme membre connecté.</summary>
    /// <param name="targetType">Type de la cible signalée.</param>
    /// <param name="targetId">Identifiant de la cible.</param>
    /// <param name="request">Motif et précisions.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le signalement enregistré.</returns>
    /// <response code="201">Signalement enregistré.</response>
    /// <response code="401">Utilisateur anonyme.</response>
    /// <response code="403">Membre sans le rôle requis.</response>
    /// <response code="409">La cible est déjà signalée par ce membre.</response>
    [HttpPost("{targetType}/{targetId:guid}")]
    [Authorize(Roles = RoleNames.Member)]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReportResponse>> Signaler(
        ReportTargetType targetType,
        Guid targetId,
        [FromBody] CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var rapporteur = RequireUser(currentUser);

        // Un même membre ne signale pas deux fois la même cible : sans cette garde, un
        // spammeur gonflerait la file de modération sans ajouter d'information.
        var dejaSignale = await unitOfWork.ReadRepository<Report>().Query
            .AnyAsync(
                report => report.TargetType == targetType
                    && report.TargetId == targetId
                    && report.ReporterId == rapporteur
                    && report.Status != ReportStatus.Resolved,
                cancellationToken);

        if (dejaSignale)
        {
            throw new DomainException("already_exists", "Vous avez déjà signalé ce contenu.");
        }

        var signalement = new Report(targetType, targetId, request.Reason, rapporteur, request.Details);

        unitOfWork.Repository<Report>().Add(signalement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Lister), null, ApiProjections.ToResponse(signalement));
    }

    /// <summary>Liste les signalements en attente de traitement.</summary>
    /// <param name="status">Filtre facultatif par état.</param>
    /// <param name="page">Numéro de page, à partir de 1.</param>
    /// <param name="pageSize">Taille de page.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Page de signalements.</returns>
    /// <response code="200">Page de signalements.</response>
    /// <response code="403">Utilisateur non administrateur.</response>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<PageResponse<ReportResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PageResponse<ReportResponse>>> Lister(
        [FromQuery] ReportStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var requete = unitOfWork.ReadRepository<Report>().Query;

        if (status is { } etat)
        {
            requete = requete.Where(report => report.Status == etat);
        }

        // Les plus anciens d'abord : un signalement attend une file de modération.
        requete = requete.OrderBy(report => report.CreatedAtUtc);

        return await PageOfAsync(
            requete,
            new PageRequest(page, pageSize),
            ApiProjections.ToResponse,
            cancellationToken);
    }

    /// <summary>Clôt un signalement comme justifié.</summary>
    /// <param name="id">Identifiant du signalement.</param>
    /// <param name="note">Note de résolution, facultative.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le signalement clôturé.</returns>
    /// <response code="200">Signalement résolu.</response>
    /// <response code="404">Signalement introuvable.</response>
    [HttpPost("{id:guid}/resolve")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> Resoudre(
        Guid id,
        [FromQuery] string? note = null,
        CancellationToken cancellationToken = default) =>
        await Cloturer(id, note, resolve: true, cancellationToken);

    /// <summary>Clôt un signalement comme non fondé.</summary>
    /// <param name="id">Identifiant du signalement.</param>
    /// <param name="note">Note de clôture, facultative.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le signalement clôturé.</returns>
    /// <response code="200">Signalement rejeté.</response>
    /// <response code="404">Signalement introuvable.</response>
    [HttpPost("{id:guid}/dismiss")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> Rejeter(
        Guid id,
        [FromQuery] string? note = null,
        CancellationToken cancellationToken = default) =>
        await Cloturer(id, note, resolve: false, cancellationToken);

    private async Task<ActionResult<ReportResponse>> Cloturer(
        Guid id,
        string? note,
        bool resolve,
        CancellationToken cancellationToken)
    {
        var moderateur = RequireUser(currentUser);

        var signalement = await unitOfWork.ReadRepository<Report>().Query
            .FirstOrDefaultAsync(report => report.Id == id, cancellationToken)
            ?? throw new DomainException("not_found", "Ce signalement n'existe pas.");

        if (resolve)
        {
            signalement.Resolve(moderateur, note, clock.GetUtcNow().UtcDateTime);
        }
        else
        {
            signalement.Dismiss(moderateur, note, clock.GetUtcNow().UtcDateTime);
        }

        unitOfWork.Repository<Report>().Update(signalement);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiProjections.ToResponse(signalement));
    }
}
