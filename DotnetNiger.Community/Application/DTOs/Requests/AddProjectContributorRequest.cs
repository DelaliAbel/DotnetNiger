using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class AddProjectContributorRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty; // Owner, Maintainer, Contributor
}
