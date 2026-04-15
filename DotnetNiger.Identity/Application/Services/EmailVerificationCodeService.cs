using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DotnetNiger.Identity.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DotnetNiger.Identity.Application.Services;

public class EmailVerificationCodeService : IEmailVerificationCodeService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789".ToCharArray();

    private readonly IDistributedCache _cache;

    public EmailVerificationCodeService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string> CreateCodeAsync(string email, string identityToken, CancellationToken ct = default)
    {
        var code = GenerateCode(6);
        var entry = new VerificationCodeEntry
        {
            Email = email.Trim().ToLowerInvariant(),
            IdentityToken = identityToken,
            CodeHash = Hash(code),
            ExpiresAtUtc = DateTime.UtcNow.Add(CodeLifetime),
            AttemptsRemaining = MaxAttempts
        };

        var payload = JsonSerializer.Serialize(entry);
        await _cache.SetStringAsync(BuildKey(email), payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = entry.ExpiresAtUtc
        }, ct);

        return code;
    }

    public async Task<string?> ConsumeIdentityTokenAsync(string email, string code, CancellationToken ct = default)
    {
        var key = BuildKey(email);
        var payload = await _cache.GetStringAsync(key, ct);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var entry = JsonSerializer.Deserialize<VerificationCodeEntry>(payload);
        if (entry == null || entry.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await _cache.RemoveAsync(key, ct);
            return null;
        }

        if (!string.Equals(entry.CodeHash, Hash(code), StringComparison.Ordinal))
        {
            entry.AttemptsRemaining--;
            if (entry.AttemptsRemaining <= 0)
            {
                await _cache.RemoveAsync(key, ct);
            }
            else
            {
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(entry), new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = entry.ExpiresAtUtc
                }, ct);
            }

            return null;
        }

        await _cache.RemoveAsync(key, ct);
        return entry.IdentityToken;
    }

    private static string BuildKey(string email) => $"verify-code:{email.Trim().ToLowerInvariant()}";

    private static string GenerateCode(int length)
    {
        Span<char> chars = stackalloc char[length];
        var random = RandomNumberGenerator.GetInt32(int.MaxValue);
        using var rng = RandomNumberGenerator.Create();
        Span<byte> bytes = stackalloc byte[length];
        rng.GetBytes(bytes);

        for (var i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }

        return new string(chars);
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input.Trim().ToUpperInvariant()));
        return Convert.ToHexString(bytes);
    }

    private sealed class VerificationCodeEntry
    {
        public string Email { get; init; } = string.Empty;
        public string CodeHash { get; init; } = string.Empty;
        public string IdentityToken { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
        public int AttemptsRemaining { get; set; }
    }
}
