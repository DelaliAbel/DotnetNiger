using System.Security.Claims;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Règles de rattachement d'une identité externe à un compte existant.
/// </summary>
public static class ExternalAccountLinking
{
    /// <summary>
    /// Revendications par lesquelles un fournisseur atteste la possession d'une adresse.
    /// </summary>
    /// <remarks>
    /// Google, Microsoft et GitHub n'utilisent pas le même nom : les deux premiers
    /// publient <c>email_verified</c>, GitHub publie <c>verified</c>. Les deux signifient
    /// la même chose, et ignorer l'un des deux interdirait aux membres GitHub de
    /// rattacher leur compte.
    /// </remarks>
    private static readonly string[] VerificationClaimTypes =
    [
        "email_verified",
        "verified",
    ];

    /// <summary>
    /// Indique si un fournisseur externe a certifié l'adresse qu'il transmet.
    /// </summary>
    /// <param name="claims">Revendications de l'identité externe.</param>
    /// <returns>
    /// <see langword="true"/> si l'adresse est certifiée, donc suffisamment fiable pour
    /// rattacher un compte existant sans confirmation supplémentaire.
    /// </returns>
    /// <remarks>
    /// Une adresse non certifiée, ou une absence de certification, vaut refus : rattacher
    /// un compte sur la seule foi d'une adresse déclarée permettrait à quiconque de s'approprier
    /// le compte d'un tiers en enregistrant cette adresse chez un fournisseur externe.
    /// </remarks>
    public static bool IsEmailVerified(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        return claims.Any(claim =>
            VerificationClaimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase)
            && (string.Equals(claim.Value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(claim.Value, "1", StringComparison.Ordinal)));
    }
}
