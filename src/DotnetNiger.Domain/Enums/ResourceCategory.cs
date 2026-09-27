namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Categories de la page Ressources. Les articles de blog ne font pas partie de cette
/// enumeration : ils ont leur propre entite.
/// </summary>
public enum ResourceCategory
{
    /// <summary>Cours en ligne, notamment Microsoft Learn.</summary>
    Training = 0,

    /// <summary>Tutoriel redige par la communaute.</summary>
    Tutorial = 1,

    /// <summary>Guide pratique ou article de reference.</summary>
    Guide = 2,

    /// <summary>Bibliotheque ou paquet NuGet.</summary>
    Library = 3,

    /// <summary>Outil, extension ou CLI.</summary>
    Tool = 4,
}
