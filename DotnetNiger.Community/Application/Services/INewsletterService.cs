using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.DTOs.Responses;
using DotnetNiger.Common.DTOs.Responses;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion des abonnements à la newsletter.</summary>
public interface INewsletterService
{
    /// <summary>Inscription d'un email (réabonne automatiquement si déjà présent et inactif).</summary>
    Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request);
    /// <summary>Désabonnement via token de sécurité.</summary>
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request);
    /// <summary>Liste paginée des abonnés.</summary>
    Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page = 1, int pageSize = 10);
    /// <summary>Nombre d'abonnés actifs.</summary>
    Task<int> GetActiveCountAsync();
}
