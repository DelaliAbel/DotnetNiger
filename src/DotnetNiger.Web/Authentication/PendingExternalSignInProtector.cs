using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Identité externe en attente, le temps que le membre choisisse un mot de passe.
/// </summary>
/// <param name="Provider">Fournisseur d'origine.</param>
/// <param name="ProviderKey">Identité du compte chez le fournisseur.</param>
/// <param name="Email">Adresse transmise par le fournisseur.</param>
/// <param name="EmailVerified">Vrai si le fournisseur a certifié l'adresse.</param>
/// <param name="ReturnUrl">Destination interne demandée avant le départ.</param>
public sealed record PendingExternalSignIn(
    string Provider,
    string ProviderKey,
    string? Email,
    bool EmailVerified,
    string? ReturnUrl);

/// <summary>
/// Transport chiffré de l'identité externe entre le retour du fournisseur et la saisie du
/// mot de passe.
/// </summary>
/// <remarks>
/// L'identité du compte chez le fournisseur ne transite pas dans la barre d'adresse. Un
/// lien fabriqué à la main pourrait rattacher le compte d'une autre personne à la session
/// du visiteur, ou faire hériter un visiteur des accès d'un tiers. Le jeton ci-dessous est
/// protégé par l'ASP.NET Core Data Protection : il est illisible et infalsifiable hors de
/// cette application.
/// </remarks>
public sealed class PendingExternalSignInProtector(
    IDataProtectionProvider provider,
    TimeProvider timeProvider)
{
    private const string Purpose = "DotnetNiger.ExternalSignIn.v1";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    private readonly IDataProtector _protector =
        provider.CreateProtector(Purpose, "pending-external-signin");

    /// <summary>
    /// Encode une identité en attente pour la transporter dans une URL.
    /// </summary>
    /// <param name="pending">Identité à transporter.</param>
    /// <returns>Jeton protégé et échappé pour la chaîne de requête.</returns>
    public string Protect(PendingExternalSignIn pending)
    {
        ArgumentNullException.ThrowIfNull(pending);

        // La protection de données garantit la confidentialité et l'intégrité du jeton, pas
        // sa durée de vie : sans cet horodatage, le lien de définition du mot de passe
        // resterait rejouable indéfiniment.
        var json = JsonSerializer.Serialize(new Envelope(pending, timeProvider.GetUtcNow()));

        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(_protector.Protect(json)));
    }

    /// <summary>
    /// Décode une identité en attente.
    /// </summary>
    /// <param name="token">Jeton reçu dans la requête.</param>
    /// <returns>L'identité transportée, ou null si le jeton est absent, expiré ou falsifié.</returns>
    public PendingExternalSignIn? Unprotect(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        Envelope envelope;

        try
        {
            var bytes = WebEncoders.Base64UrlDecode(token);
            var json = _protector.Unprotect(Encoding.UTF8.GetString(bytes));

            envelope = JsonSerializer.Deserialize<Envelope>(json)
                ?? throw new JsonException("Enveloppe vide.");
        }
        catch (Exception exception) when (exception is CryptographicException
                                       or FormatException
                                       or JsonException
                                       or ArgumentException)
        {
            // Un jeton invalide est traité comme un jeton absent : l'utilisateur recommence.
            return null;
        }

        if (timeProvider.GetUtcNow() - envelope.IssuedAt > Lifetime)
        {
            return null;
        }

        return envelope.Pending;
    }

    /// <summary>
    /// Contenu réellement protégé : l'identité et l'instant d'émission du jeton.
    /// </summary>
    /// <param name="Pending">Identité en attente.</param>
    /// <param name="IssuedAt">Instant d'émission, servant à la péremption.</param>
    private sealed record Envelope(PendingExternalSignIn Pending, DateTimeOffset IssuedAt);
}
