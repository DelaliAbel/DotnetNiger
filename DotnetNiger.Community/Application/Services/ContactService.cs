using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Infrastructure;

namespace DotnetNiger.Community.Application.Services;

public class ContactService(AppDbContext db) : IContactService
{
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
