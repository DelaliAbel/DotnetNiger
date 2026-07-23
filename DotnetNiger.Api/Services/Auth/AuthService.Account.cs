using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Api.Services.Auth;

/// <summary>Générateur de codes aléatoires pour les confirmations.</summary>
internal static class CodeGenerator
{
    private static readonly char[] CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    /// <summary>Génère un code aléatoire de 6 caractères.</summary>
    public static string Generate()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(6);
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = CodeChars[bytes[i] % CodeChars.Length];
        return new string(code);
    }
}

/// <summary>Gestion de l'historique des connexions utilisateur.</summary>
public partial class AuthService
{
    /// <summary>Enregistre une tentative de connexion dans l'historique.</summary>
    public async Task RecordLoginAsync(Guid userId, string ipAddress, string userAgent, bool success, string? failureReason = null)
    {
        var entry = new LoginHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason
        };
        _db.LoginHistories.Add(entry);
        await _db.SaveChangesAsync();
    }

    /// <summary>Récupère l'historique des connexions d'un utilisateur avec pagination.</summary>
    public async Task<object> GetLoginHistoryAsync(Guid userId, int page, int pageSize)
    {
        var query = _db.LoginHistories.Where(h => h.UserId == userId);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                h.Id, h.IpAddress, h.UserAgent, h.Success, h.FailureReason, h.CreatedAt
            })
            .ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
