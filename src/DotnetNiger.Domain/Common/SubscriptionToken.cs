using System.Security.Cryptography;
using System.Text;

namespace DotnetNiger.Domain.Common;

/// <summary>
/// Produit et vérifie les jetons d'abonnement à la lettre d'information.
/// </summary>
/// <remarks>
/// Un jeton d'abonnement n'est pas un mot de passe : rien n'est secret derrière lui, et il
/// n'est valable que pour une seule adresse. Il n'a pas besoin d'être devinable en
/// revanche — un jeton prévisible permettrait à un tiers de confirmer l'inscription d'une
/// adresse qui n'a rien demandé, donc de la soumettre à des messages non désirés.
/// <para>
/// Seule l'empreinte est conservée en base. Le jeton voyage dans le lien du courriel, et la
/// base ne permet pas de le reconstituer.
/// </para>
/// </remarks>
public static class SubscriptionToken
{
    /// <summary>
    /// Produit un jeton et son empreinte.
    /// </summary>
    /// <returns>Le jeton à envoyer, et l'empreinte à conserver.</returns>
    public static (string Jeton, string Empreinte) Creer()
    {
        var jeton = Base64Url(RandomNumberGenerator.GetBytes(32));
        return (jeton, Hasher(jeton));
    }

    /// <summary>
    /// Produit la clé de désabonnement d'un abonné.
    /// </summary>
    /// <returns>Clé à conserver et à reprendre dans chaque lettre envoyée.</returns>
    /// <remarks>
    /// Contrairement au jeton de confirmation, cette clé est conservée en clair et sert
    /// durablement : chaque lettre doit porter le lien de désabonnement de son destinataire,
    /// or une empreinte ne peut pas être remise dans un lien. Elle n'est pas un secret de
    /// verrouillage — elle n'accorde aucun accès, elle ne fait que retirer un adresse de la
    /// liste, et elle ne quitte que la boîte de réception de cette adresse.
    /// </remarks>
    public static string CreerCleDesabonnement() => Base64Url(RandomNumberGenerator.GetBytes(32));

    /// <summary>
    /// Calcule l'empreinte d'un jeton présenté par un visiteur.
    /// </summary>
    /// <param name="token">Jeton présenté dans le lien.</param>
    /// <returns>L'empreinte à comparer à celle conservée.</returns>
    public static string Hasher(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
