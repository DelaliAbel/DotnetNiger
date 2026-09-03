using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Api.Application.DTOs.Requests;
using DotnetNiger.Api.Application.DTOs.Responses;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using DotnetNiger.Api.Infrastructure.Email;

namespace DotnetNiger.Api.Application.Services.Newsletter;

/// <summary>Service de gestion des inscriptions à la newsletter.</summary>
public class NewsletterService : INewsletterService
{
    private const int ConfirmationTokenLifetimeHours = 24;

    private readonly DotnetNigerDbContext _db;
    private readonly IEmailService _emailService;
    private readonly SmtpOptions _smtp;

    public NewsletterService(DotnetNigerDbContext db, IEmailService emailService, IOptions<SmtpOptions> smtp)
    {
        _db = db;
        _emailService = emailService;
        _smtp = smtp.Value;
    }

    /// <summary>Inscrit un email à la newsletter (initie le double opt-in si non confirmé).</summary>
    public async Task<NewsletterSubscriptionResponse> SubscribeAsync(SubscribeRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var existing = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == email, ct);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UnsubscribedAt = null;
                existing.IsConfirmed = false;
                existing.ConfirmedAt = null;
                existing.ConfirmationToken = GenerateToken();
                existing.ConfirmationExpiresAt = DateTime.UtcNow.AddHours(ConfirmationTokenLifetimeHours);
                existing.SubscribedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
                await SendConfirmationEmailAsync(existing, ct);
            }
            else if (!existing.IsConfirmed)
            {
                existing.ConfirmationToken = GenerateToken();
                existing.ConfirmationExpiresAt = DateTime.UtcNow.AddHours(ConfirmationTokenLifetimeHours);
                await _db.SaveChangesAsync(ct);
                await SendConfirmationEmailAsync(existing, ct);
            }
            return MapToResponse(existing);
        }

        var sub = new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = string.IsNullOrWhiteSpace(request.Name) ? string.Empty : request.Name.Trim(),
            UnsubscribeToken = GenerateToken(),
            ConfirmationToken = GenerateToken(),
            ConfirmationExpiresAt = DateTime.UtcNow.AddHours(ConfirmationTokenLifetimeHours),
            IsConfirmed = false,
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        };
        _db.Set<NewsletterSubscription>().Add(sub);
        await _db.SaveChangesAsync(ct);
        await SendConfirmationEmailAsync(sub, ct);
        return MapToResponse(sub);
    }

    /// <summary>Confirme l'inscription d'un abonné via son token.</summary>
    public async Task<bool> ConfirmSubscriptionAsync(string token, CancellationToken ct = default)
    {
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.ConfirmationToken == token, ct);
        if (sub == null) return false;
        if (sub.ConfirmationExpiresAt.HasValue && sub.ConfirmationExpiresAt < DateTime.UtcNow) return false;

        sub.IsConfirmed = true;
        sub.ConfirmedAt = DateTime.UtcNow;
        sub.ConfirmationToken = null;
        sub.ConfirmationExpiresAt = null;
        sub.IsActive = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Désinscrit un email de la newsletter via un token.</summary>
    public async Task<bool> UnsubscribeAsync(UnsubscribeRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == email && s.UnsubscribeToken == request.Token, ct);
        if (sub == null) return false;
        sub.IsActive = false;
        sub.UnsubscribedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Supprime définitivement une inscription par email.</summary>
    public async Task<bool> DeleteByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = NormalizeEmail(email);
        var sub = await _db.Set<NewsletterSubscription>()
            .FirstOrDefaultAsync(s => s.Email == normalized, ct);
        if (sub == null) return false;
        _db.Set<NewsletterSubscription>().Remove(sub);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Récupère la liste paginée des inscriptions.</summary>
    public async Task<PaginatedResponse<NewsletterSubscriptionResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Set<NewsletterSubscription>().AsNoTracking();
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return new PaginatedResponse<NewsletterSubscriptionResponse>(
            items.Select(MapToResponse).ToList(), total, page, pageSize);
    }

    /// <summary>Retourne le nombre d'inscriptions actives.</summary>
    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
    {
        return await _db.Set<NewsletterSubscription>().CountAsync(s => s.IsActive, ct);
    }

    /// <summary>Envoie une newsletter à tous les abonnés actifs et confirmés.</summary>
    public async Task<NewsletterSendResponse> SendAsync(SendNewsletterRequest request, CancellationToken ct = default)
    {
        var recipients = await _db.Set<NewsletterSubscription>()
            .AsNoTracking()
            .Where(s => s.IsActive && s.IsConfirmed)
            .Select(s => s.Email)
            .ToListAsync(ct);

        if (recipients.Count == 0)
            return new NewsletterSendResponse(0);

        var htmlBody = BuildNewsletterBody(request.Content);
        await _emailService.SendBatchAsync(recipients.ToArray(), request.Subject, htmlBody);
        return new NewsletterSendResponse(recipients.Count);
    }

    private async Task SendConfirmationEmailAsync(NewsletterSubscription sub, CancellationToken ct)
    {
        var confirmUrl = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/newsletter/confirm?token={Uri.EscapeDataString(sub.ConfirmationToken!)}";
        await _emailService.SendEmailAsync(sub.Email, "Confirmez votre inscription à la newsletter", BuildConfirmationBody(sub, confirmUrl));
    }

    private string BuildConfirmationBody(NewsletterSubscription sub, string confirmUrl)
    {
        var greeting = string.IsNullOrWhiteSpace(sub.Name) ? "Bonjour" : $"Bonjour {sub.Name}";
        var body = $@"
<p>{greeting},</p>
<p>Merci de vous être inscrit(e) à la newsletter de {_smtp.AppName}. Veuillez confirmer votre inscription en cliquant sur le bouton ci-dessous :</p>
<p><a href=""{confirmUrl}"" style=""display:inline-block;padding:12px 28px;background:#0067b8;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600"">Confirmer mon inscription</a></p>
<p>Ce lien expirera dans 24 heures. Si vous n'êtes pas à l'origine de cette inscription, vous pouvez ignorer cet email.</p>";
        return body;
    }

    private string BuildNewsletterBody(string content)
    {
        return $@"
<p>{content.Replace("\n", "<br/>")}</p>
<p style=""margin-top:24px;font-size:12px;color:#999999"">Vous recevez cet email car vous êtes abonné(e) à la newsletter de {_smtp.AppName}.
<a href=""{_smtp.FrontendBaseUrl.TrimEnd('/')}/newsletter/unsubscribe"">Se désabonner</a>.</p>";
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateToken() => Guid.NewGuid().ToString("N");

    private static NewsletterSubscriptionResponse MapToResponse(NewsletterSubscription s) =>
        new(s.Id, s.Email, s.Name, s.IsConfirmed, s.IsActive, s.SubscribedAt);
}
