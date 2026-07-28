namespace DotnetNiger.Api.Application.DTOs.Responses;

/// <summary>Paramètres publics du site exposés via l'API anonyme.</summary>
public class PublicSettingsResponse
{
    public string SiteName { get; set; } = ".NET Niger";
    public string DefaultOgImage { get; set; } = "/images/og-default.jpg";
    public string LogoUrl { get; set; } = "";
    public string ContactEmail { get; set; } = "";
}
