using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Infrastructure;

namespace DotnetNiger.Community.Application.Services;

/// <summary>Enregistre les messages du formulaire de contact en base.</summary>
public class ContactService(AppDbContext db) : IContactService
{
    /// <summary>Sauvegarde un message de contact et le marque comme non lu.</summary>
    public async Task<bool> SendAsync(ContactRequest request)
    {
        var message = new ContactMessage
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        db.ContactMessages.Add(message);
        await db.SaveChangesAsync();
        return true;
    }
}
