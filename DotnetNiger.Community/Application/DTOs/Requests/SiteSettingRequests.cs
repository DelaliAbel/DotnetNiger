namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête de mise à jour d'un paramètre du site.</summary>
public class UpdateSiteSettingRequest
{
    public string Value { get; set; } = string.Empty;
}

/// <summary>Requête de mise à jour en masse des paramètres du site.</summary>
public class UpdateSiteSettingsRequest
{
    public Dictionary<string, string> Settings { get; set; } = new();
}
