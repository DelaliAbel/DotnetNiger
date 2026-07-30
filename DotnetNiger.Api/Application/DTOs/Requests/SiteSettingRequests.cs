namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête de mise à jour d'un paramètre du site.</summary>
public class UpdateSiteSettingRequest
{
    /// <summary>Nouvelle valeur du paramètre.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>Requête de mise à jour groupée des paramètres du site.</summary>
public class UpdateSiteSettingsRequest
{
    /// <summary>Dictionnaire clé-valeur des paramètres à mettre à jour.</summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}
