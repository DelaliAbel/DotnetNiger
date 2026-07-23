namespace DotnetNiger.Api.Entities;

/// <summary>
/// Représente une compétence associée à un membre.
/// </summary>
public class MemberSkill
{
    /// <summary>Identifiant unique de la compétence.</summary>
    public Guid Id { get; set; }
    /// <summary>Identifiant du membre.</summary>
    public Guid MemberId { get; set; }
    /// <summary>Nom de la compétence.</summary>
    public string SkillName { get; set; } = string.Empty;
    /// <summary>Nom de la compétence (variant).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Navigation vers le membre.</summary>
    public Member Member { get; set; } = null!;
}
