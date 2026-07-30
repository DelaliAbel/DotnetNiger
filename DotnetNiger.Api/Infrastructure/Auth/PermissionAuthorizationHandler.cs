using Microsoft.AspNetCore.Authorization;

namespace DotnetNiger.Api.Infrastructure.Auth;

/// <summary>Exigence d'autorisation basée sur une permission spécifique.</summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission) => Permission = permission;
}

/// <summary>Gestionnaire d'autorisation vérifiant les permissions via les claims.</summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>Vérifie si l'utilisateur possède la permission requise.</summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
