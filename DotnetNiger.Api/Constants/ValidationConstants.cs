namespace DotnetNiger.Api.Constants;

/// <summary>
/// Constantes de validation (tailles maximales, pagination, etc.).
/// </summary>
public static class ValidationConstants
{
    /// <summary>Nombre max d'éléments par page.</summary>
    public const int MaxPageSize = 100;
    /// <summary>Nombre min d'éléments par page.</summary>
    public const int MinPageSize = 1;
    /// <summary>Nombre par défaut d'éléments par page.</summary>
    public const int DefaultPageSize = 10;
    /// <summary>Longueur max d'un nom.</summary>
    public const int MaxNameLength = 100;
    /// <summary>Longueur max d'un email.</summary>
    public const int MaxEmailLength = 256;
    /// <summary>Longueur max d'un slug.</summary>
    public const int MaxSlugLength = 200;
    /// <summary>Longueur max d'un titre.</summary>
    public const int MaxTitleLength = 200;
    /// <summary>Longueur max du contenu.</summary>
    public const int MaxContentLength = 10000;
    /// <summary>Taille max d'un upload (4 Mo).</summary>
    public const int MaxUploadSize = 4 * 1024 * 1024;
}
