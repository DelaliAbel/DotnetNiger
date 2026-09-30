namespace DotnetNiger.Domain.Common;

/// <summary>
/// Limites de longueur partagées par le domaine, les configurations EF Core et la
/// validation des requêtes, afin qu'une seule valeur fasse autorité.
/// </summary>
public static class FieldLimits
{
    /// <summary>Longueur maximale d'un titre.</summary>
    public const int Title = 200;

    /// <summary>
    /// Longueur maximale d'un libellé court : couleur de badge, type de rubrique,
    /// ou tout identifiant destiné à être choisi dans une liste plutôt que saisi.
    /// </summary>
    public const int Label = 50;

    /// <summary>Longueur maximale d'un slug.</summary>
    public const int Slug = 200;

    /// <summary>Longueur maximale d'un chapo.</summary>
    public const int Excerpt = 500;

    /// <summary>Longueur maximale d'un corps de contenu riche.</summary>
    public const int Content = 32_000;

    /// <summary>Longueur maximale d'un corps de texte court (commentaire, message).</summary>
    public const int Body = 4_000;

    /// <summary>Longueur maximale d'une URL.</summary>
    public const int Url = 500;

    /// <summary>Longueur maximale d'une biographie.</summary>
    public const int Bio = 2_000;

    /// <summary>Longueur maximale d'un nom de personne ou d'organisation.</summary>
    public const int Name = 150;

    /// <summary>Longueur maximale d'un identifiant de compte externe (GitHub, LinkedIn...).</summary>
    public const int Handle = 100;

    /// <summary>Longueur maximale d'une ville ou d'une localisation.</summary>
    public const int Location = 150;

    /// <summary>Longueur maximale d'une liste de technologies séparées par des virgules.</summary>
    public const int Technologies = 300;

    /// <summary>Longueur maximale d'un numéro de téléphone.</summary>
    public const int Phone = 30;

    /// <summary>Longueur maximale d'une adresse de courrier électronique.</summary>
    public const int EmailAddress = 320;

    /// <summary>Longueur maximale de l'objet d'un courriel.</summary>
    public const int Subject = 200;

    /// <summary>Longueur maximale d'une clé technique (modèle de courriel, type de notification).</summary>
    public const int Key = 80;
}
