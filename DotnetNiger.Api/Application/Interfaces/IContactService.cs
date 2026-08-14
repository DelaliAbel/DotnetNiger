using System.Threading;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;

namespace DotnetNiger.Api.Application.Interfaces;

/// <summary>Interface du service de messages de contact.</summary>
public interface IContactService
{
    /// <summary>Envoie un message de contact.</summary>
    Task<bool> SendAsync(ContactRequest request, CancellationToken ct = default);
    /// <summary>Récupère tous les messages de contact (admin).</summary>
    Task<List<ContactMessageResponse>> GetAllAsync(CancellationToken ct = default);
    /// <summary>Marque un message comme lu.</summary>
    Task<bool> MarkAsReadAsync(Guid id, CancellationToken ct = default);
}
