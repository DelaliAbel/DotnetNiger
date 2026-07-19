using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class ContactService : IContactService
{
    private readonly DotnetNigerDbContext _db;

    public ContactService(DotnetNigerDbContext db) => _db = db;

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
