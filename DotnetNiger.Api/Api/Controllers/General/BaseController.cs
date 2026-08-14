using System.Security.Claims;
using DotnetNiger.Api.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Api.Controllers;

/// <summary>Contrôleur de base avec des utilitaires d'authentification partagés.</summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>Récupère l'identifiant de l'utilisateur connecté.</summary>
    protected Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException(Messages.User.InvalidIdentity);
        return userId;
    }

    /// <summary>Vérifie si l'utilisateur connecté est administrateur.</summary>
    protected bool IsAdmin() =>
        User.IsInRole(RoleConstants.Admin) || User.IsInRole(RoleConstants.SuperAdmin);

    /// <summary>Vérifie si l'utilisateur connecté est collaborateur.</summary>
    protected bool IsCollaborator() =>
        User.IsInRole(RoleConstants.Collaborator);

    /// <summary>Récupère le nom complet de l'utilisateur connecté.</summary>
    protected string GetUserName() =>
        User.FindFirstValue("full_name")
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)
        ?? User.FindFirstValue(ClaimTypes.Name)
        ?? "Inconnu";

    /// <summary>Récupère l'URL de l'avatar de l'utilisateur connecté.</summary>
    protected string GetUserAvatar() =>
        User.FindFirstValue("avatar_url") ?? string.Empty;

    protected IActionResult Success<T>(T data, string? message = null) =>
        Ok(new { success = true, data, message });

    protected IActionResult Created<T>(T data, string? message = null) =>
        StatusCode(201, new { success = true, data, message });

    protected IActionResult Failure(string message, int statusCode = 400) =>
        StatusCode(statusCode, new { success = false, data = (object?)null, message });

    protected IActionResult NotFound(string message = "Ressource non trouvée") =>
        NotFound(new { success = false, data = (object?)null, message });

    protected IActionResult BadRequest(string message) =>
        BadRequest(new { success = false, data = (object?)null, message });

    protected IActionResult Error(string message, List<string>? errors = null) =>
        BadRequest(new { success = false, data = (object?)null, message, errors });
}
