using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.General;

/// <summary>Interface du service de gestion des paramètres du site.</summary>
public interface ISettingsService
{
    /// <summary>Récupère tous les paramètres.</summary>
    Task<List<SiteSettingResponse>> GetAllAsync();
    /// <summary>Récupère un paramètre par clé.</summary>
    Task<SiteSettingResponse?> GetByKeyAsync(string key);
    /// <summary>Définit un paramètre par clé/valeur.</summary>
    Task<SiteSettingResponse> SetAsync(string key, string value);
    /// <summary>Définit plusieurs paramètres.</summary>
    Task SetBatchAsync(Dictionary<string, string> settings);
    /// <summary>Supprime un paramètre.</summary>
    Task<bool> DeleteAsync(string key);
}
