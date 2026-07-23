using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.General;

/// <summary>Service de gestion des messages de contact.</summary>
public class ContactService : IContactService
{
    private readonly DotnetNigerDbContext _db;

    public ContactService(DotnetNigerDbContext db) => _db = db;

    /// <summary>Enregistre un message de contact en base de données.</summary>
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
        _db.Set<ContactMessage>().Add(message);
        await _db.SaveChangesAsync();
        return true;
    }
}
