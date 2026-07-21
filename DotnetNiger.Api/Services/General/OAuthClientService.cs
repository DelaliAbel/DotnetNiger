using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;

namespace DotnetNiger.Api.Services.General;

public class OAuthClientService
{
    private readonly DotnetNigerDbContext _db;

    public OAuthClientService(DotnetNigerDbContext db) => _db = db;

    public async Task<PaginatedResponse<OAuthClientResponse>> GetClientsAsync(PaginationQuery pagination)
    {
        var query = _db.OAuthClients.AsNoTracking();
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .ToListAsync();
        return new PaginatedResponse<OAuthClientResponse>(
            items.Select(MapToResponse).ToList(), totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    public async Task<List<OAuthClientResponse>> GetClientsByClientIdAsync(string clientId)
    {
        var clients = await _db.OAuthClients
            .AsNoTracking()
            .Where(c => c.ClientId == clientId).ToListAsync();
        return clients.Select(MapToResponse).ToList();
    }

    public async Task<OAuthClientResponse> GetClientByIdAsync(Guid clientId)
    {
        var client = await _db.OAuthClients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId)
            ?? throw new KeyNotFoundException("Client OAuth non trouvé");
        return MapToResponse(client);
    }

    private static OAuthClientResponse MapToResponse(OAuthClient c) =>
        new(c.Id, c.ClientId, c.ClientName, c.Description,
            DeserializeJsonArray(c.RedirectUris),
            DeserializeJsonArray(c.PostLogoutRedirectUris),
            DeserializeJsonArray(c.AllowedGrantTypes),
            c.IsActive, c.CreatedAt);

    internal static List<string> ParseJsonArray(string? json) =>
        string.IsNullOrWhiteSpace(json) ? [] :
        JsonSerializer.Deserialize<List<string>>(json) ?? [];

    internal static List<string> ParseJsonArrayOrDefault(string? json, string[] defaults) =>
        string.IsNullOrWhiteSpace(json) ? [.. defaults] : ParseJsonArray(json);

    private static List<string> DeserializeJsonArray(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];
}
