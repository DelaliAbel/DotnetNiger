namespace DotnetNiger.Community.Domain.Entities;

public class TeamMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // FK Identity API
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty; // Lead, Organizer, Mentor, Editor, Ambassador
    public int Order { get; set; } // Pour l'ordre d'affichage
    public string BioOverride { get; set; } = string.Empty; // Bio spécifique au team
    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string RoleDescription { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Relations
    public ICollection<TeamMemberSkill> Skills { get; set; } = new List<TeamMemberSkill>();
}
