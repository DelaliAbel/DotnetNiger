using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Infrastructure.Email.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Api.Application.Services.Contact;

/// <summary>Service de gestion des messages de contact.</summary>
public class ContactService : IContactService
{
    private readonly DotnetNigerDbContext _db;
    private readonly IEmailService _email;
    private readonly SmtpOptions _smtp;

    public ContactService(DotnetNigerDbContext db, IEmailService email, IOptions<SmtpOptions> smtp)
    {
        _db = db;
        _email = email;
        _smtp = smtp.Value;
    }

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
        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync();

        if (!string.IsNullOrEmpty(_smtp.SupportEmail))
        {
            var (subject, body) = ContactNotificationTemplate.Render(
                request.FullName, request.Email, request.Subject, request.Message);
            await _email.SendEmailAsync(_smtp.SupportEmail, subject, body, request.Email);
        }

        return true;
    }

    /// <summary>Récupère tous les messages de contact (admin).</summary>
    public async Task<List<ContactMessageResponse>> GetAllAsync()
    {
        return await _db.ContactMessages.AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ContactMessageResponse
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Subject = m.Subject,
                Message = m.Message,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead
            })
            .ToListAsync();
    }

    /// <summary>Marque un message de contact comme lu.</summary>
    public async Task<bool> MarkAsReadAsync(Guid id)
    {
        var message = await _db.ContactMessages.FindAsync(id);
        if (message == null) return false;
        message.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
