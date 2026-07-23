using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;

namespace DotnetNiger.Api.Services.General;

/// <summary>Interface du service de gestion de la newsletter.</summary>
public interface INewsletterService
{
    /// <summary>Inscrit un email à la newsletter.</summary>
    Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request);
    /// <summary>Désinscrit un email de la newsletter.</summary>
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request);
    /// <summary>Supprime une inscription par email.</summary>
    Task<bool> DeleteByEmailAsync(string email);
    /// <summary>Récupère les inscriptions paginées.</summary>
    Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize);
    /// <summary>Retourne le nombre d'inscriptions actives.</summary>
    Task<int> GetActiveCountAsync();
}
