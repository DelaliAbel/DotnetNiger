namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Format d'un evenement de la communaute.
/// </summary>
public enum CommunityEventFormat
{
    /// <summary>Evenement physique, une adresse est obligatoire.</summary>
    InPerson = 0,

    /// <summary>Evenement en ligne, un lien est obligatoire.</summary>
    Online = 1,

    /// <summary>Evenement physique et en ligne : adresse et lien obligatoires.</summary>
    Hybrid = 2,
}
