using System.Security.Claims;
using DotnetNiger.Common.Constants;
using DotnetNiger.Community.Application.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Community.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>Extrait et valide l'identifiant de l'utilisateur authentifié depuis les claims JWT.</summary>
    protected Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException(Messages.User.InvalidIdentity);
        return userId;
    }

    /// <summary>Retourne vrai si l'utilisateur courant est Admin ou SuperAdmin.</summary>
    protected bool IsAdmin() =>
        User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.SuperAdmin);

    /// <summary>Retourne vrai si l'utilisateur courant est un Collaborateur.</summary>
    protected bool IsCollaborator() =>
        User.IsInRole(RoleConstants.Collaborator);

    /// <summary>Récupère le nom d'affichage depuis les claims JWT, avec repli sur le nom d'utilisateur.</summary>
    protected string GetUserName() =>
        User.FindFirstValue("full_name") ?? User.FindFirstValue(ClaimTypes.Name) ?? "Inconnu";

    /// <summary>Récupère l'URL de l'avatar depuis les claims JWT.</summary>
    protected string GetUserAvatar() =>
        User.FindFirstValue("avatar_url") ?? string.Empty;
}
