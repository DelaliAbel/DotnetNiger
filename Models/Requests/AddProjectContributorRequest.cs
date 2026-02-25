using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.UI.Models.Requests;

public class AddProjectContributorRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty; // Owner, Maintainer, Contributor
}
