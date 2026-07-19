namespace DotnetNiger.Common.Constants;

/// <summary>
/// Constantes de validation utilisées dans toute l'application.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// Taille maximale d'une page de résultats (pagination).
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Taille minimale d'une page de résultats.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// Taille de page par défaut.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Longueur maximale d'un nom d'utilisateur.
    /// </summary>
    public const int MaxNameLength = 100;

    /// <summary>
    /// Longueur maximale d'un email.
    /// </summary>
    public const int MaxEmailLength = 256;

    /// <summary>
    /// Longueur maximale d'un slug.
    /// </summary>
    public const int MaxSlugLength = 200;

    /// <summary>
    /// Longueur maximale d'un titre.
    /// </summary>
    public const int MaxTitleLength = 200;

    /// <summary>
    /// Longueur maximale d'un contenu textuel.
    /// </summary>
    public const int MaxContentLength = 10000;

    /// <summary>
    /// Taille maximale d'un fichier uploadé en octets (4 Mo).
    /// </summary>
    public const int MaxUploadSize = 4 * 1024 * 1024;
}
