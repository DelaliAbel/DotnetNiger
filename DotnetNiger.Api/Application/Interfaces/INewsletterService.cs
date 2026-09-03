using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de gestion de la newsletter.</summary>
public interface INewsletterService
{
    /// <summary>Inscrit un email à la newsletter (initie le double opt-in).</summary>
    Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request, CancellationToken ct = default);
    /// <summary>Confirme l'inscription d'un abonné via son token (double opt-in).</summary>
    Task<bool> ConfirmSubscriptionAsync(string token, CancellationToken ct = default);
    /// <summary>Désinscrit un email de la newsletter.</summary>
    Task<bool> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken ct = default);
    /// <summary>Supprime une inscription par email.</summary>
    Task<bool> DeleteByEmailAsync(string email, CancellationToken ct = default);
    /// <summary>Récupère les inscriptions paginées.</summary>
    Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    /// <summary>Retourne le nombre d'inscriptions actives.</summary>
    Task<int> GetActiveCountAsync(CancellationToken ct = default);
    /// <summary>Envoie une newsletter à tous les abonnés actifs et confirmés.</summary>
    Task<NewsletterSendResponse> SendAsync(SendNewsletterRequest request, CancellationToken ct = default);
}
