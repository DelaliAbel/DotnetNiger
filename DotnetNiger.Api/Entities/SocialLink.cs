namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente un lien vers un réseau social d'un membre.
/// </summary>
public class SocialLink
{
    /// <summary>Identifiant unique du lien.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant du membre.</summary>
    public Guid MemberId { get; set; }
    /// <summary>Nom de la plateforme (GitHub, LinkedIn, etc.).</summary>
    public string Platform { get; set; } = string.Empty;
    /// <summary>URL du profil.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Navigation vers le membre.</summary>
    public Member Member { get; set; } = null!;
}
