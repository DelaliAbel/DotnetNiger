using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Common.DTOs.Requests;
using DotnetNiger.Common.DTOs.Responses;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Application.Services;

public class TenantClientService
{
    private readonly IdentityDbContext _db;

    public TenantClientService(IdentityDbContext db) => _db = db;

    public async Task<PaginatedResponse<TenantClientResponse>> GetClientsAsync(Guid tenantId, PaginationQuery pagination)
    {
        var query = _db.TenantClients.AsNoTracking().Where(c => c.TenantId == tenantId);
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pagination.EnsurePage - 1) * pagination.EnsurePageSize)
            .Take(pagination.EnsurePageSize)
            .ToListAsync();
        return new PaginatedResponse<TenantClientResponse>(
            items.Select(MapToResponse).ToList(), totalCount, pagination.EnsurePage, pagination.EnsurePageSize);
    }

    public async Task<List<TenantClientResponse>> GetClientsByClientIdAsync(string clientId)
    {
        var clients = await _db.TenantClients
            .AsNoTracking().IgnoreQueryFilters()
            .Where(c => c.ClientId == clientId).ToListAsync();
        return clients.Select(MapToResponse).ToList();
    }

    public async Task<TenantClientResponse> GetClientByIdAsync(Guid tenantId, Guid clientId)
    {
        var client = await _db.TenantClients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Client OAuth non trouvé");
        return MapToResponse(client);
    }

    private static TenantClientResponse MapToResponse(TenantClient c) =>
        new(c.Id, c.TenantId, c.ClientId, c.ClientName, c.Description,
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
