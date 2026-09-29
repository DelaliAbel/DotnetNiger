using DotnetNiger.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Web.Controller.Common;

/// <summary>
/// Traduit les erreurs métier du domaine en réponses HTTP.
/// </summary>
/// <remarks>
/// Une <see cref="DomainException"/> n'est pas une panne : c'est une règle métier refusée.
/// La laisser remonter produirait une erreur 500, qui ferait croire à un bug et exposerait
/// une pile d'appels. Le format retenu est celui de la RFC 9457, déjà utilisé par le reste
/// de l'application, avec le code métier conservé dans une extension.
/// </remarks>
public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException requeteInvalide)
        {
            return await RepondreBadRequest(httpContext, requeteInvalide, cancellationToken);
        }

        if (exception is not DomainException domain)
        {
            return false;
        }

        ApiLog.RegleMetierRefusee(
            logger,
            httpContext.Request.Path,
            domain.Code,
            domain.Message);

        httpContext.Response.StatusCode = StatusFor(domain.Code);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Title = TitleFor(httpContext.Response.StatusCode),
                Detail = domain.Message,
                Type = $"https://dotnetniger.local/erreurs/{domain.Code}",
            },
        });
    }

    /// <summary>
    /// Traduit une requête mal formée en 400 plutôt qu'en 500.
    /// </summary>
    /// <param name="httpContext">Requête en cours.</param>
    /// <param name="exception">Erreur levée pendant la lecture de la requête.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Vrai si la réponse a été écrite.</returns>
    /// <remarks>
    /// C'est le cas le plus courant sur les pages de compte : un formulaire dont le jeton
    /// antiforgerie est absent, expiré ou falsifié. Le jeton est validé avant même
    /// d'atteindre le point de terminaison, et l'exception montre que c'est la requête,
    /// non le serveur, qui est en faute. Laissée en 500, elle afficherait une page
    /// d'erreur à un membre dont la session venait simplement d'expirer, et noierait le
    /// suivi des pannes sous chaque formulaire abandonné. Le message reste volontairement
    /// bref : le détail technique n'expose rien d'exploitable mais invite à le contourner.
    /// </remarks>
    private async ValueTask<bool> RepondreBadRequest(
        HttpContext httpContext,
        BadHttpRequestException exception,
        CancellationToken cancellationToken)
    {
        // Un jeton antiforgerie rejeté peut signaler une tentative de contournement, pas
        // seulement un formulaire abandonné : le niveau Avertissement se justifie.
        ApiLog.RequeteMalFormee(
            logger,
            httpContext.Request.Path,
            exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Requête refusée",
                Detail = "Le formulaire n'a pas pu être validé. Rechargez la page et réessayez.",
                Type = "https://dotnetniger.local/erreurs/requete_invalide",
            },
        });
    }

    /// <summary>
    /// Déduit le code HTTP du code métier.
    /// </summary>
    /// <param name="code">Code métier stable.</param>
    /// <returns>Code HTTP correspondant.</returns>
    /// <remarks>
    /// Le domaine ne connaît pas HTTP : la correspondance est donc définie ici, et
    /// regroupée par intention plutôt que dispersée dans chaque point de terminaison.
    /// </remarks>
    private static int StatusFor(string code) => code switch
    {
        "not_found" => StatusCodes.Status404NotFound,
        "already_exists" or "slug_already_used" => StatusCodes.Status409Conflict,
        "forbidden" => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest,
    };

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status404NotFound => "Ressource introuvable",
        StatusCodes.Status409Conflict => "Conflit avec l'état actuel",
        StatusCodes.Status403Forbidden => "Action non autorisée",
        _ => "Requête refusée par une règle métier",
    };
}
