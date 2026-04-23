namespace DotnetNiger.Community.Domain.Entities;

using DotnetNiger.Community.Domain.Enums;
using DotnetNiger.Community.Domain.Interfaces;

/// <summary>
/// Represents a core team member (e.g. Lead, Organizer, Mentor) of the community.
/// Users from Identity API represent the general audience.
/// </summary>
public class TeamMember : ISoftDeletable
{
    /// <summary>Gets or sets the internal unique identifier for the team member record.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the Identity API User ID associated with this team member.</summary>
    public Guid UserId { get; set; } // FK Identity API

    /// <summary>Gets or sets the display name for the team member.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the position/role title (e.g. Lead, Organizer, Mentor, Editor, Ambassador).</summary>
    public string Position { get; set; } = string.Empty; // Lead, Organizer, Mentor, Editor, Ambassador

    /// <summary>Gets or sets the order in which to display the team member on the UI.</summary>
    public int Order { get; set; } // Pour l'ordre d'affichage

    /// <summary>Gets or sets a specific bio that overrides the general user bio.</summary>
    public string BioOverride { get; set; } = string.Empty; // Bio spÃ©cifique au team

    /// <summary>Gets or sets a value indicating whether this team member profile is publicly visible.</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the team member is currently active in the community.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the membership approval status (Pending, Approved, Rejected).</summary>
    public ApprovalStatus MembershipStatus { get; set; } = ApprovalStatus.Pending;

    /// <summary>Gets or sets the date and time when this membership was reviewed.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Gets or sets the reviewer user id from Identity API.</summary>
    public Guid? ReviewedByUserId { get; set; }

    /// <summary>Gets or sets a longer description of the team member's responsibilities or role.</summary>
    public string RoleDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time when the member joined the core team.</summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether this member has been soft deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC date/time of soft deletion.</summary>
    public DateTime? DeletedAt { get; set; }

    // Relations
    /// <summary>Gets or sets the specific skills associated with this team member.</summary>
    public ICollection<TeamMemberSkill> Skills { get; set; } = new List<TeamMemberSkill>();
}
