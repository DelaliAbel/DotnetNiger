namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>
/// Request for updating an existing member.
/// </summary>
public class UpdateTeamMemberRequest
{
    /// <summary>Gets or sets the member's name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the member's position/role in team.</summary>
    public string? Position { get; set; }

    /// <summary>Gets or sets the member's bio override.</summary>
    public string? BioOverride { get; set; }

    /// <summary>Gets or sets the display order.</summary>
    public int? Order { get; set; }

    /// <summary>Gets or sets whether member is public.</summary>
    public bool? IsPublic { get; set; }

    /// <summary>Gets or sets whether member is active.</summary>
    public bool? IsActive { get; set; }

    /// <summary>Gets or sets the role description.</summary>
    public string? RoleDescription { get; set; }
}
