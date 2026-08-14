using System.Threading;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion des paramètres du site.</summary>
public interface ISettingsService
{
    /// <summary>Récupère tous les paramètres.</summary>
    Task<List<SiteSettingResponse>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Récupère un paramètre par clé.</summary>
    Task<SiteSettingResponse?> GetByKeyAsync(string key, CancellationToken ct = default);
    /// <summary>Définit un paramètre par clé/valeur.</summary>
    Task<SiteSettingResponse> SetAsync(string key, string value, CancellationToken ct = default);
    /// <summary>Définit plusieurs paramètres.</summary>
    Task SetBatchAsync(Dictionary<string, string> settings, CancellationToken ct = default);
    /// <summary>Supprime un paramètre.</summary>
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);
    /// <summary>Récupère les paramètres publics du site.</summary>
    Task<PublicSettingsResponse> GetPublicSettingsAsync(CancellationToken ct = default);
}
