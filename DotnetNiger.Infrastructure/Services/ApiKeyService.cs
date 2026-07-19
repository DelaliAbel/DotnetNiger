using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly DotnetNigerDbContext _db;

    public ApiKeyService(DotnetNigerDbContext db) => _db = db;

    public async Task<ApiKeyCreatedResponse> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        var secret = GenerateSecret();
        var prefix = "dni_" + secret[..Math.Min(8, secret.Length)].ToLowerInvariant();

        var publicKey = $"pk_{Guid.NewGuid():N}";
        var privateKeyHash = HashSecret(secret);

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            KeyPrefix = prefix,
            PublicKey = publicKey,
            PrivateKeyHash = privateKeyHash,
            Scopes = string.IsNullOrWhiteSpace(request.Scopes)
                ? "[\"api\"]"
                : request.Scopes,
            IsActive = true,
            ExpiresAt = request.ExpiresAt,
        };

        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync();

        return new ApiKeyCreatedResponse(
            MapToResponse(apiKey),
            secret);
    }

    public async Task<PaginatedResponse<ApiKeyResponse>> GetApiKeysAsync(PaginationQuery pagination)
    {
        var query = _db.ApiKeys.AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(k => k.CreatedAt)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .ToListAsync();

        return new PaginatedResponse<ApiKeyResponse>(
            items.Select(MapToResponse).ToList(), totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    public async Task<ApiKeyResponse> GetApiKeyByIdAsync(Guid keyId)
    {
        var key = await _db.ApiKeys.AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == keyId)
            ?? throw new KeyNotFoundException("Clé API non trouvée");

        return MapToResponse(key);
    }

    public async Task DeleteApiKeyAsync(Guid keyId)
    {
        var key = await _db.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == keyId)
            ?? throw new KeyNotFoundException("Clé API non trouvée");

        _db.ApiKeys.Remove(key);
        await _db.SaveChangesAsync();
    }

    public async Task<ApiKeyCreatedResponse> RotateApiKeyAsync(Guid keyId)
    {
        var key = await _db.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == keyId)
            ?? throw new KeyNotFoundException("Clé API non trouvée");

        var secret = GenerateSecret();
        var prefix = "dni_" + secret[..Math.Min(8, secret.Length)].ToLowerInvariant();
        var privateKeyHash = HashSecret(secret);

        key.KeyPrefix = prefix;
        key.PrivateKeyHash = privateKeyHash;
        key.LastUsedAt = null;

        await _db.SaveChangesAsync();

        return new ApiKeyCreatedResponse(
            MapToResponse(key),
            secret);
    }

    private static ApiKeyResponse MapToResponse(ApiKey k)
    {
        return new ApiKeyResponse(
            k.Id, k.Name, k.KeyPrefix, k.PublicKey,
            JsonSerializer.Deserialize<List<string>>(k.Scopes) ?? ["api"],
            k.IsActive, k.CreatedAt, k.ExpiresAt, k.LastUsedAt);
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(bytes);
    }
}
