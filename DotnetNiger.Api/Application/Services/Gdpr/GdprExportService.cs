using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;

namespace DotnetNiger.Api.Application.Services.Gdpr;

/// <summary>Export RGPD et droit à l'effacement (ForgetMe).</summary>
public class GdprExportService
{
    private readonly DotnetNigerDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public GdprExportService(DotnetNigerDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    /// <summary>Exporte toutes les données personnelles de l'utilisateur au format ZIP.</summary>
    public async Task<(byte[] ZipData, string UserName)> ExportUserDataAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new KeyNotFoundException(ErrorMessages.UserNotFound);

        var userName = $"{user.FirstName}-{user.LastName}".Trim('-');
        var roles = await _userManager.GetRolesAsync(user);
        var consents = await _db.UserConsents.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync();
        var auditLogs = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt).Take(500).ToListAsync();


        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            await AddJsonToArchive(archive, "profile.json", new
            {
                user.Id, user.UserName, user.Email, user.FirstName,
                user.LastName, user.AvatarUrl,
                user.IsActive, user.EmailConfirmed,
                user.CreatedAt
            });

            await AddJsonToArchive(archive, "roles.json", roles);

            if (consents.Count != 0)
                await AddJsonToArchive(archive, "consents.json", consents);

            if (auditLogs.Count != 0)
                await AddJsonToArchive(archive, "audit-logs.json", auditLogs);

        }

        return (memoryStream.ToArray(), userName);
    }

    /// <summary>Anonymise les données personnelles (droit à l'effacement RGPD).</summary>
    public async Task ForgetMeAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new KeyNotFoundException(ErrorMessages.UserNotFound);

        var anonymizedEmail = $"anonymized-{userId:N}@dotnetniger.com";
        user.Email = anonymizedEmail;
        user.UserName = anonymizedEmail;
        user.FirstName = "Anonymized";
        user.LastName = "User";
        user.AvatarUrl = null;
        user.IsActive = false;
        user.EmailConfirmed = false;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationCodeExpiry = null;
        user.PhoneNumber = null;
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count != 0)
            await _userManager.RemoveFromRolesAsync(user, roles);

        var logins = await _userManager.GetLoginsAsync(user);
        foreach (var login in logins)
            await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);

        await _db.AuditLogs
            .Where(a => a.UserId == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Description, "[anonymized]")
                .SetProperty(a => a.IpAddress, (string?)null));
                          
        var oldConsents = await _db.UserConsents
            .Where(c => c.UserId == userId && c.CreatedAt < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();
        _db.UserConsents.RemoveRange(oldConsents);

        await _db.SaveChangesAsync();
    }

    private static async Task AddJsonToArchive(ZipArchive archive, string entryName, object data)
    {
        var entry = archive.CreateEntry(entryName);
        using var writer = new StreamWriter(entry.Open());
        await writer.WriteAsync(JsonSerializer.Serialize(data,
            new JsonSerializerOptions { WriteIndented = true }));
    }
}
