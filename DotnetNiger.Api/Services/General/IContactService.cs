using DotnetNiger.Api.DTOs.Requests;

namespace DotnetNiger.Api.Services.General;

/// <summary>Interface du service de messages de contact.</summary>
public interface IContactService
{
    /// <summary>Envoie un message de contact.</summary>
    Task<bool> SendAsync(ContactRequest request);
}
