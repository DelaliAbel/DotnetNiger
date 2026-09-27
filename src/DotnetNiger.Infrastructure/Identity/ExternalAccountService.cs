using System.Security.Claims;
using DotnetNiger.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Traitement des retours de connexion depuis Google, Microsoft ou GitHub.
/// </summary>
/// <remarks>
/// Règle appliquée : l'adresse d'un compte est un identifiant, pas une preuve. Un compte
/// local et une identité externe ne sont donc jamais fusionnés sur la seule foi d'une
/// adresse déclarée. Le rattachement n'a lieu que si le fournisseur a certifié
/// l'adresse, ou si la personne prouve elle-même le compte en saisissant son mot de passe.
/// </remarks>
public sealed partial class ExternalAccountService(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ExternalAccountService> logger) : IExternalAccountService
{
    private const string DisplayNameClaim = "name";

    /// <inheritdoc />
    public async Task<ExternalSignInResult> Complete(
        string provider,
        string providerKey,
        string? email,
        IEnumerable<Claim> claims)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);
        ArgumentNullException.ThrowIfNull(claims);

        var claimList = claims as IReadOnlyList<Claim> ?? [.. claims];
        var normalizedEmail = email?.Trim();
        var emailVerified = ExternalAccountLinking.IsEmailVerified(claimList);

        // 1. Identité déjà rattachée : chemin ordinaire, sans risque.
        var linked = await userManager.FindByLoginAsync(provider, providerKey);
        if (linked is not null)
        {
            return new ExternalSignInResult(
                ExternalSignInStatus.Connected,
                linked,
                normalizedEmail,
                emailVerified);
        }

        var existing = string.IsNullOrWhiteSpace(normalizedEmail)
            ? null
            : await userManager.FindByEmailAsync(normalizedEmail);

        // 2. Adresse non certifiée : aucun rattachement automatique. Un compte portant
        //    cette adresse existe peut-être ; seule la saisie du mot de passe le prouve.
        if (!emailVerified)
        {
            LogRattachementSuspendu(logger, provider);

            return new ExternalSignInResult(
                existing is null
                    ? ExternalSignInStatus.PasswordRequired
                    : ExternalSignInStatus.PasswordProofRequired,
                existing,
                normalizedEmail,
                emailVerified);
        }

        // 3. Adresse certifiée et compte existant : rattachement, sans ressaisie.
        if (existing is not null)
        {
            var link = await userManager.AddLoginAsync(
                existing,
                new UserLoginInfo(provider, providerKey, ReadDisplayName(claimList, provider)));

            if (link.Succeeded)
            {
                return new ExternalSignInResult(
                    ExternalSignInStatus.LinkedToExistingAccount,
                    existing,
                    normalizedEmail,
                    emailVerified);
            }

            LogRattachementRefuse(
                logger,
                provider,
                string.Join(" ; ", link.Errors.Select(erreur => erreur.Description)));

            return new ExternalSignInResult(
                ExternalSignInStatus.PasswordProofRequired,
                existing,
                normalizedEmail,
                emailVerified);
        }

        // 4. Identité inconnue et adresse certifiée : aucun rattachement n'est possible,
        //    puisqu'il n'existe pas encore de compte. Le compte n'est pas créé ici malgré la
        //    certification : le membre doit d'abord choisir un mot de passe.
        //
        //    Créer le compte maintenant laisserait une ligne sans `PasswordHash`, donc un
        //    compte que le site ne peut authentifier autrement que par ce fournisseur, et dont
        //    la saisie du mot de passe viendrait ensuite buter sur un doublon d'adresse. La
        //    création est donc déléguée à CreateWithPasswordAsync, qui reçoit le mot de passe.
        //
        //    L'adresse étant certifiée, ce compte sera confirmé sans courriel de confirmation.
        return new ExternalSignInResult(
            ExternalSignInStatus.PasswordRequired,
            null,
            normalizedEmail,
            emailVerified);
    }

    /// <inheritdoc />
    public async Task<IdentityResult> CreateWithPasswordAsync(
        ApplicationUser user,
        string password,
        string provider,
        string providerKey,
        string? providerDisplayName)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);

        var creation = await userManager.CreateAsync(user, password);

        if (!creation.Succeeded)
        {
            return creation;
        }

        var role = await userManager.AddToRoleAsync(user, RoleNames.Member);

        if (!role.Succeeded)
        {
            return await RollbackAsync(
                user,
                "Le rôle initial n'a pas pu être attribué",
                string.Join(" ; ", role.Errors.Select(erreur => erreur.Description)));
        }

        var link = await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(provider, providerKey, providerDisplayName ?? provider));

        if (!link.Succeeded)
        {
            // Un compte créé sans son identité externe rattachée n'a pas de sens : le membre
            // reviendrait chez le fournisseur, le service le retrouverait sans pouvoir le
            // rattacher, et l'adresse resterait occupée par un compte orphelin. Il est donc
            // supprimé, pour que la saisie du mot de passe puisse être recommencée.
            return await RollbackAsync(
                user,
                "L'identité externe n'a pas pu être rattachée",
                string.Join(" ; ", link.Errors.Select(erreur => erreur.Description)));
        }

        return creation;
    }

    /// <summary>
    /// Supprime un compte vient d'être créé mais laissé incomplet, et renvoie l'échec au
    /// membre pour qu'il puisse recommencer.
    /// </summary>
    /// <param name="user">Compte à supprimer.</param>
    /// <param name="motif">Motif renvoyé au membre.</param>
    /// <param name="detail">Cause technique, journalisée.</param>
    /// <returns>Un résultat en échec.</returns>
    private async Task<IdentityResult> RollbackAsync(
        ApplicationUser user,
        string motif,
        string detail)
    {
        LogRattachementRefuse(logger, motif, detail);

        var deletion = await userManager.DeleteAsync(user);

        if (!deletion.Succeeded)
        {
            // Le compte reste en base. Il n'est ni connecté ni rattaché, et l'adresse est
            // signalée pour correction manuelle plutôt que d'être masquée.
            LogSuppressionImpossible(
                logger,
                user.Email ?? "(adresse inconnue)",
                string.Join(" ; ", deletion.Errors.Select(erreur => erreur.Description)));
        }

        return IdentityResult.Failed(new IdentityError { Code = "AccountNotCreated", Description = motif });
    }

    /// <inheritdoc />
    public async Task<IdentityResult> LinkAsync(
        string provider,
        string providerKey,
        string? providerDisplayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerKey);

        var user = await GetAuthenticatedUserAsync();

        return await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(provider, providerKey, providerDisplayName ?? provider));
    }

    /// <summary>
    /// Récupère le compte de la session courante.
    /// </summary>
    /// <returns>Le compte connecté.</returns>
    /// <exception cref="InvalidOperationException">Aucune session n'est ouverte.</exception>
    public async Task<ApplicationUser> GetAuthenticatedUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException(
                "Aucun utilisateur connecté : le rattachement d'un compte externe exige une session ouverte.");

        return await userManager.GetUserAsync(principal)
            ?? throw new InvalidOperationException(
                "Aucun utilisateur connecté : le rattachement d'un compte externe exige une session ouverte.");
    }

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Rattachement suspendu : l'adresse fournie par {Provider} n'est pas certifiée par le fournisseur.")]
    private static partial void LogRattachementSuspendu(ILogger logger, string provider);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "Rattachement du compte existant refusé pour {Provider} : {Motif}")]
    private static partial void LogRattachementRefuse(ILogger logger, string provider, string motif);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Warning,
        Message = "Compte créé mais incomplet pour {Email} : suppression impossible ({Detail})")]
    private static partial void LogSuppressionImpossible(
        ILogger logger,
        string email,
        string detail);

    private static string? ReadDisplayName(IReadOnlyList<Claim> claims, string provider)
    {
        var nom = claims.FirstOrDefault(claim =>
            string.Equals(claim.Type, DisplayNameClaim, StringComparison.OrdinalIgnoreCase))?.Value;

        return string.IsNullOrWhiteSpace(nom) ? provider : nom;
    }
}
