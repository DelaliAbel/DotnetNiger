using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Domain.Entities.Content;

/// <summary>
/// Abonnement d'une adresse à la lettre d'information.
/// </summary>
/// <remarks>
/// L'abonné n'est pas nécessairement un membre inscrit : la lettre vise aussi les personnes
/// qui suivent l'association sans avoir de compte. L'adresse n'est donc pas une clé
/// étrangère, et l'abonnement survit à la suppression d'un compte comme à sa création.
/// <para>
/// Deux identités distincts, aux durées de vie différentes, servent au même abonnement.
/// </para>
/// <para>
/// Le jeton de confirmation n'est jamais stocké en clair : seule son empreinte est conservée,
/// de sorte qu'une fuite de la base ne permette pas de confirmer l'abonnement d'une adresse
/// qui n'a rien demandé — ce qui la soumettrait ensuite à des messages qu'elle n'a pas
/// voulus. Il ne sert qu'une fois et meurt à la confirmation.
/// </para>
/// <para>
/// La clé de désabonnement, elle, doit être reprise dans chaque lettre et ne peut donc pas
/// être sous forme d'empreinte. Elle est conservée en clair, ce qui se décide : une fuite de
/// la base permettrait alors de désabonner des gens, mais ne révélerait ni leur adresse ni le
/// contenu des lettres, et surtout le lien voyage vers la seule boîte qui doit le recevoir.
/// </para>
/// </remarks>
public sealed class NewsletterSubscription : TimestampedEntity
{
    /// <summary>
    /// Constructeur sans paramètre réservé à Entity Framework Core.
    /// </summary>
    private NewsletterSubscription()
    {
    }

    private NewsletterSubscription(
        string email,
        string tokenHash,
        string unsubscribeKey,
        DateTime requestedAtUtc)
    {
        Email = DomainGuard.AgainstTooLong(
            DomainGuard.AgainstNullOrWhiteSpace(email, nameof(email)),
            FieldLimits.EmailAddress,
            nameof(email));

        TokenHash = DomainGuard.AgainstNullOrWhiteSpace(tokenHash, nameof(tokenHash));
        UnsubscribeKey = DomainGuard.AgainstNullOrWhiteSpace(unsubscribeKey, nameof(unsubscribeKey));
        RequestedAtUtc = requestedAtUtc;
        Status = NewsletterStatus.Pending;
    }

    /// <summary>
    /// Enregistre une demande d'abonnement en attente de confirmation.
    /// </summary>
    /// <param name="email">Adresse demandée.</param>
    /// <param name="tokenHash">Empreinte du jeton envoyé par courriel.</param>
    /// <param name="unsubscribeKey">Clé de désabonnement, reprise dans chaque lettre.</param>
    /// <param name="requestedAtUtc">Instant de la demande (UTC).</param>
    /// <returns>Nouvel abonnement, en attente.</returns>
    public static NewsletterSubscription Create(
        string email,
        string tokenHash,
        string unsubscribeKey,
        DateTime requestedAtUtc) =>
        new(email, tokenHash, unsubscribeKey, requestedAtUtc);

    /// <summary>
    /// Obtient l'adresse abonnée.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'état de l'abonnement.
    /// </summary>
    public NewsletterStatus Status { get; private set; }

    /// <summary>
    /// Obtient l'empreinte du jeton en cours de validité.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient la clé de désabonnement, à insérer en clair dans chaque lettre.
    /// </summary>
    public string UnsubscribeKey { get; private set; } = string.Empty;

    /// <summary>
    /// Obtient l'instant de la demande (UTC).
    /// </summary>
    public DateTime RequestedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'instant de confirmation (UTC), ou <see langword="null"/>.
    /// </summary>
    public DateTime? ConfirmedAtUtc { get; private set; }

    /// <summary>
    /// Obtient l'instant du désabonnement (UTC), ou <see langword="null"/>.
    /// </summary>
    public DateTime? UnsubscribedAtUtc { get; private set; }

    /// <summary>
    /// Confirme l'abonnement si le jeton présenté correspond à celui en cours.
    /// </summary>
    /// <param name="presentedTokenHash">Empreinte du jeton présenté par le visiteur.</param>
    /// <param name="nowUtc">Instant de la confirmation (UTC).</param>
    /// <returns><see langword="true"/> si l'abonnement vient d'être confirmé.</returns>
    /// <remarks>
    /// Confirmer deux fois ne doit pas écraser l'instant de la première confirmation :
    /// celui-ci date l'origine de l'abonnement.
    /// </remarks>
    public bool Confirm(string presentedTokenHash, DateTime nowUtc)
    {
        if (!JetonValide(presentedTokenHash) || Status == NewsletterStatus.Confirmed)
        {
            return false;
        }

        Status = NewsletterStatus.Confirmed;
        ConfirmedAtUtc ??= nowUtc;
        UnsubscribedAtUtc = null;

        return true;
    }

    /// <summary>
    /// Désabonne l'adresse si le jeton présenté correspond à celui en cours.
    /// </summary>
    /// <param name="presentedKey">Clé de désabonnement présentée par le visiteur.</param>
    /// <param name="nowUtc">Instant du désabonnement (UTC).</param>
    /// <returns><see langword="true"/> si l'abonnement vient d'être rompu.</returns>
    public bool Unsubscribe(string presentedKey, DateTime nowUtc)
    {
        if (!CleDesabonnementValide(presentedKey) || Status == NewsletterStatus.Unsubscribed)
        {
            return false;
        }

        Status = NewsletterStatus.Unsubscribed;
        UnsubscribedAtUtc = nowUtc;

        return true;
    }

    /// <summary>
    /// Remet l'abonnement en attente de confirmation, avec un jeton neuf.
    /// </summary>
    /// <param name="tokenHash">Empreinte du nouveau jeton.</param>
    /// <param name="requestedAtUtc">Instant de la nouvelle demande (UTC).</param>
    /// <remarks>
    /// Le jeton change à chaque nouvelle demande : un lien de confirmation ancien ne doit pas
    /// pouvoir confirmer un abonnement refermé entre-temps.
    /// </remarks>
    public void RequestAgain(string tokenHash, DateTime requestedAtUtc)
    {
        TokenHash = DomainGuard.AgainstNullOrWhiteSpace(tokenHash, nameof(tokenHash));
        Status = NewsletterStatus.Pending;
        RequestedAtUtc = requestedAtUtc;
        UnsubscribedAtUtc = null;
    }

    /// <summary>
    /// Indique si l'adresse doit recevoir la lettre.
    /// </summary>
    public bool IsConfirmed => Status == NewsletterStatus.Confirmed;

    private bool JetonValide(string presentedTokenHash) => Egal(TokenHash, presentedTokenHash);

    private bool CleDesabonnementValide(string presentedKey) => Egal(UnsubscribeKey, presentedKey);

    private static bool Egal(string attendu, string presente) =>
        !string.IsNullOrWhiteSpace(presente)
        && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(attendu),
            System.Text.Encoding.UTF8.GetBytes(presente));
}
