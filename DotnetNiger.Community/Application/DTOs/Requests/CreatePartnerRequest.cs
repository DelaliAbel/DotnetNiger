using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class CreatePartnerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? PartnerType { get; set; }
    public string? Level { get; set; }
}
