using System.Globalization;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Domain.Common;

/// <summary>
/// Contrôles d'invariant appliqués à la construction et aux mutations des entités.
/// </summary>
/// <remarks>
/// Ces contrôles levant <see cref="DomainException"/> become des réponses HTTP 4xx.
/// Les erreurs de programmation (null, type invalide) restent des
/// <see cref="ArgumentException"/>.
/// </remarks>
public static class DomainGuard
{
    /// <summary>
    /// Vérifie qu'une référence obligatoire est renseignée.
    /// </summary>
    /// <typeparam name="T">Type de la référence.</typeparam>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>La référence, non nulle.</returns>
    /// <exception cref="DomainException">La référence est <see langword="null"/>.</exception>
    /// <remarks>
    /// Une entité associée — une rubrique, un membre — ne peut pas être absente : la
    /// laisser à <see langword="null"/> déplacerait l'échec au moment de la lecture, en
    /// <c>NullReferenceException</c>, loin de sa cause. Le garde-fou transforme ce cas en
    /// erreur de domaine, au moment où l'entité est construite.
    /// </remarks>
    public static T AgainstNull<T>(T? value, string field)
        where T : class
    {
        if (value is null)
        {
            throw new DomainException(
                "required_reference_missing",
                $"{field} est obligatoire.");
        }

        return value;
    }

    /// <summary>
    /// Vérifie qu'un texte obligatoire est renseigné et le renvoie normalisé.
    /// </summary>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Texte trimmed.</returns>
    /// <exception cref="DomainException">Le texte est vide.</exception>
    public static string AgainstNullOrWhiteSpace(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("field_required", $"Le champ « {field} » est obligatoire.");
        }

        return value.Trim();
    }

    /// <summary>
    /// Vérifie qu'un texte optionnel ne dépasse pas la longueur maximale.
    /// </summary>
    /// <param name="value">Valeur à contrôler, éventuellement <see langword="null"/>.</param>
    /// <param name="maxLength">Longueur maximale.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Texte trimmed, ou <see langword="null"/>.</returns>
    /// <exception cref="DomainException">Le texte est trop long.</exception>
    public static string? OptionalAgainstTooLong(string? value, int maxLength, string field)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        return trimmed is null ? null : AgainstTooLong(trimmed, maxLength, field);
    }

    /// <summary>
    /// Vérifie qu'un texte ne dépasse pas la longueur maximale.
    /// </summary>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="maxLength">Longueur maximale.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Texte inchangé.</returns>
    /// <exception cref="DomainException">Le texte est trop long.</exception>
    public static string AgainstTooLong(string value, int maxLength, string field)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException(
                "field_too_long",
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"Le champ « {field} » ne doit pas dépasser {maxLength} caractères (reçu : {value.Length})."));
        }

        return value;
    }

    /// <summary>
    /// Vérifie qu'un identifiant est renseigné.
    /// </summary>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Identifiant validé.</returns>
    /// <exception cref="DomainException">L'identifiant est <see cref="Guid.Empty"/>.</exception>
    public static Guid AgainstEmpty(Guid value, string field)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("field_required", $"Le champ « {field} » est obligatoire.");
        }

        return value;
    }

    /// <summary>
    /// Vérifie qu'un nombre est positif ou nul.
    /// </summary>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Valeur validée.</returns>
    /// <exception cref="DomainException">La valeur est négative.</exception>
    public static int AgainstNegative(int value, string field)
    {
        if (value < 0)
        {
            throw new DomainException("field_negative", $"Le champ « {field} » ne peut pas être négatif.");
        }

        return value;
    }

    /// <summary>
    /// Vérifie qu'une URL est absolue et limitée aux schémas HTTP et HTTPS.
    /// </summary>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>URL validée.</returns>
    /// <exception cref="DomainException">
    /// L'URL est absente, relative, trop longue ou utilise un autre schéma que HTTP ou HTTPS.
    /// </exception>
    /// <remarks>
    /// Le contrôle de schéma est une protection contre les URL de type <c>javascript:</c>
    /// stockées en base puis rendues dans un attribut <c>href</c>.
    /// </remarks>
    public static string AgainstHttpUrl(string? value, string field)
    {
        var url = AgainstNullOrWhiteSpace(value, field);
        AgainstTooLong(url, FieldLimits.Url, field);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException(
                "field_invalid_url",
                $"Le champ « {field} » doit être une URL absolue commençant par http:// ou https://.");
        }

        return url;
    }

    /// <summary>
    /// Vérifie qu'une URL optionnelle est valide si elle est renseignée.
    /// </summary>
    /// <param name="value">Valeur à contrôler, éventuellement <see langword="null"/>.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>URL validée, ou <see langword="null"/>.</returns>
    /// <exception cref="DomainException">L'URL est renseignée mais invalide.</exception>
    public static string? OptionalAgainstHttpUrl(string? value, string field)
        => string.IsNullOrWhiteSpace(value) ? null : AgainstHttpUrl(value, field);

    /// <summary>
    /// Vérifie qu'une image est référencée soit par une URL absolue HTTP ou HTTPS,
    /// soit par un chemin du site.
    /// </summary>
    /// <param name="value">Valeur à contrôler.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Adresse validée.</returns>
    /// <exception cref="DomainException">
    /// L'adresse est absente, trop longue, ou n'est ni une URL HTTP(S) ni un chemin du site.
    /// </exception>
    /// <remarks>
    /// Les images déposées par l'administration sont servies par le site lui-même, à
    /// une adresse de la forme <c>/api/images/…</c>. Les accepter évite d'écrire le nom
    /// du domaine dans chaque enregistrement : un changement d'adresse ne casserait pas
    /// les images déjà en ligne.
    /// <para>
    /// La protection reste entière. Un chemin ne peut pas porter de schéma, donc il ne
    /// peut pas être un <c>javascript:</c> ; le risque que la garde cherche à écarter —
    /// une adresse stockée puis rendue dans un attribut — disparaît donc avec l'exigence
    /// d'absolu.
    /// </para>
    /// </remarks>
    public static string AgainstImageSource(string? value, string field)
    {
        var source = AgainstNullOrWhiteSpace(value, field);
        AgainstTooLong(source, FieldLimits.Url, field);

        if (EstCheminDuSite(source))
        {
            return source;
        }

        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException(
                "field_invalid_url",
                $"Le champ « {field} » doit être une URL absolue http(s) ou un chemin du site.");
        }

        return source;
    }

    /// <summary>
    /// Indique si une adresse est un chemin servi par le site lui-même.
    /// </summary>
    /// <remarks>
    /// Le chemin commence par une barre oblique et ne contient aucun deux-points : un
    /// chemin n'a jamais besoin d'un, et son absence écarte « javascript:… » comme
    /// « data:… », qui sont précisément ce que la garde cherche à refusée.
    /// </remarks>
    /// <param name="source">Adresse à examiner.</param>
    /// <returns><see langword="true"/> si l'adresse est un chemin du site.</returns>
    private static bool EstCheminDuSite(string source)
        => source.StartsWith('/') && !source.Contains(':', StringComparison.Ordinal);

    /// <summary>
    /// Vérifie qu'une image est correctement référencée si elle l'est.
    /// </summary>
    /// <param name="value">Valeur à contrôler, éventuellement <see langword="null"/>.</param>
    /// <param name="field">Nom du champ, utilisé dans le code d'erreur.</param>
    /// <returns>Adresse validée, ou <see langword="null"/>.</returns>
    /// <exception cref="DomainException">L'adresse est renseignée mais invalide.</exception>
    public static string? OptionalAgainstImageSource(string? value, string field)
        => string.IsNullOrWhiteSpace(value) ? null : AgainstImageSource(value, field);
}
