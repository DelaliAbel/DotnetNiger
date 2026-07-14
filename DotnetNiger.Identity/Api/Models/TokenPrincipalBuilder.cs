using System.Security.Claims;
using DotnetNiger.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DotnetNiger.Identity.Api.Models;

public static class TokenPrincipalBuilder
{
    public static void SetUserClaims(ClaimsPrincipal principal, ApplicationUser user, IList<string> roles)
    {
        principal.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        var identity = principal.Identities.FirstOrDefault();
        if (identity != null)
        {
            foreach (var oldClaim in identity.FindAll(ClaimTypes.Role).ToList())
                identity.RemoveClaim(oldClaim);
            foreach (var oldClaim in identity.FindAll("role").ToList())
                identity.RemoveClaim(oldClaim);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                identity.AddClaim(new Claim("role", role));
            }
        }
        principal.SetClaim("tenant_id", user.TenantId.ToString());
        principal.SetClaim(OpenIddictConstants.Claims.GivenName, user.FirstName);
        principal.SetClaim(OpenIddictConstants.Claims.FamilyName, user.LastName);
        principal.SetClaim(OpenIddictConstants.Claims.Name, $"{user.FirstName} {user.LastName}".Trim());
        principal.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
    }

    public static void SetCommonDestinations(ClaimsPrincipal principal)
    {
        principal.SetDestinations(claim => claim.Type switch
        {
            Claims.Subject
                => [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Name or Claims.Email or Claims.GivenName or Claims.FamilyName
                => [Destinations.AccessToken, Destinations.IdentityToken],
            ClaimTypes.Role or "role"
                => [Destinations.AccessToken, Destinations.IdentityToken],
            "tenant_id"
                => [Destinations.AccessToken],
            _ => [Destinations.AccessToken]
        });
    }

    public static void SetUserScopes(ClaimsPrincipal principal, HttpRequest request, string defaultScopes = "openid profile email roles offline_access")
    {
        var scopes = request.Form["scope"];
        principal.SetScopes(scopes.Count > 0
            ? scopes.SelectMany(s => (s ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
            : defaultScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
