using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Résultat d'un changement de mot de passe demandé par lien.
/// </summary>
/// <param name="IsSuccess">Vrai si le mot de passe a été remplacé.</param>
/// <param name="User">Compte concerné.</param>
/// <param name="Error">Message d'échec.</param>
public sealed record PasswordResetResult(bool IsSuccess, ApplicationUser? User, string? Error)
{
    /// <summary>
    /// Crée un résultat réussi.
    /// </summary>
    /// <param name="user">Compte dont le mot de passe a été remplacé.</param>
    /// <returns>Résultat réussi.</returns>
    public static PasswordResetResult Success(ApplicationUser user) => new(true, user, null);

    /// <summary>
    /// Crée un résultat en échec.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <returns>Résultat en échec.</returns>
    public static PasswordResetResult Failure(string error) => new(false, null, error);
}

/// <summary>
/// Résultat d'une preuve de propriété d'un compte.
/// </summary>
/// <param name="IsSuccess">Vrai si le rattachement a eu lieu.</param>
/// <param name="User">Compte concerné.</param>
/// <param name="Error">Message d'échec.</param>
public sealed record PasswordProofResult(bool IsSuccess, ApplicationUser? User, string? Error)
{
    /// <summary>
    /// Crée un résultat réussi.
    /// </summary>
    /// <param name="user">Compte rattaché.</param>
    /// <returns>Résultat réussi.</returns>
    public static PasswordProofResult Success(ApplicationUser user) => new(true, user, null);

    /// <summary>
    /// Crée un résultat en échec.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <returns>Résultat en échec.</returns>
    public static PasswordProofResult Failure(string error) => new(false, null, error);
}

/// <summary>
/// Résultat de la création d'un compte depuis un fournisseur externe.
/// </summary>
/// <param name="IsSuccess">Vrai si le compte a été créé.</param>
/// <param name="User">Compte créé.</param>
/// <param name="EmailVerified">Vrai si l'adresse a été certifiée par le fournisseur.</param>
/// <param name="Error">Message d'échec.</param>
public sealed record ExternalAccountResult(
    bool IsSuccess,
    ApplicationUser? User,
    bool EmailVerified,
    string? Error)
{
    /// <summary>
    /// Crée un résultat réussi.
    /// </summary>
    /// <param name="user">Compte créé.</param>
    /// <param name="emailVerified">Vrai si l'adresse a été certifiée.</param>
    /// <returns>Résultat réussi.</returns>
    public static ExternalAccountResult Success(ApplicationUser user, bool emailVerified) =>
        new(true, user, emailVerified, null);

    /// <summary>
    /// Crée un résultat en échec.
    /// </summary>
    /// <param name="error">Motif de l'échec.</param>
    /// <returns>Résultat en échec.</returns>
    public static ExternalAccountResult Failure(string error) => new(false, null, false, error);
}