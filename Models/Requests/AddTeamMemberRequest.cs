using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class AddTeamMemberRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Position { get; set; } = string.Empty;

    public string BioOverride { get; set; } = string.Empty;

    public string RoleDescription { get; set; } = string.Empty;

    public int Order { get; set; } = 0;

    public bool IsPublic { get; set; } = true;

    public List<string> Skills { get; set; } = new();
}
