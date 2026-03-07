using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Community.Application.DTOs.Requests;

public class AddPartnerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string LogoUrl { get; set; } = string.Empty;

    [Required]
    public string Website { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string PartnerType { get; set; } = string.Empty; // Partner, Sponsor

    [Required]
    public string Level { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;
}
