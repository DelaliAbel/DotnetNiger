using DotnetNiger.Community.Application.DTOs;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Gestion du formulaire de contact.</summary>
public interface IContactService
{
    /// <summary>Enregistre un message envoyé via le formulaire de contact.</summary>
    Task<bool> SendAsync(ContactRequest request);
}
