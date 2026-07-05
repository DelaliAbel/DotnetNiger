using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Application.Services;

public partial class OpenIddictClientManager
{
    private readonly IdentityDbContext _db;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public OpenIddictClientManager(IdentityDbContext db, IOpenIddictApplicationManager applicationManager)
    {
        _db = db;
        _applicationManager = applicationManager;
    }

    public async Task<TenantClientCreatedResponse> CreateClientAsync(Guid tenantId, CreateTenantClientRequest request)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant non trouvé");

        var clientId = $"app_{Guid.NewGuid():N}";
        var clientSecret = GenerateSecret();

        var descriptor = BuildClientDescriptor(request, clientId, clientSecret);
        if (await _applicationManager.FindByClientIdAsync(clientId) != null)
            throw new InvalidOperationException($"Un client avec l'identifiant {clientId} existe déjà.");

        var app = await _applicationManager.CreateAsync(descriptor)
            ?? throw new InvalidOperationException("Échec de la création de l'application OpenIddict.");

        var tenantClient = await SaveTenantClientAsync(tenantId, request, clientId, clientSecret, app);

        return new TenantClientCreatedResponse(MapToResponse(tenantClient), clientSecret);
    }

    private async Task<TenantClient> SaveTenantClientAsync(Guid tenantId, CreateTenantClientRequest request, string clientId, string clientSecret, object app)
    {
        var grantTypes = TenantClientService.ParseJsonArrayOrDefault(request.AllowedGrantTypes,
            ["authorization_code", "password", "refresh_token", "client_credentials"]);
        var redirectUris = TenantClientService.ParseJsonArray(request.RedirectUris);
        var postLogoutUris = TenantClientService.ParseJsonArray(request.PostLogoutRedirectUris);

        var tenantClient = new TenantClient
        {
            Id = Guid.NewGuid(), TenantId = tenantId,
            ApplicationId = (await _applicationManager.GetIdAsync(app))!,
            ClientId = clientId,
            ClientSecretHash = HashSecret(clientSecret),
            ClientName = request.ClientName, Description = request.Description,
            RedirectUris = JsonSerializer.Serialize(redirectUris),
            PostLogoutRedirectUris = JsonSerializer.Serialize(postLogoutUris),
            AllowedGrantTypes = JsonSerializer.Serialize(grantTypes),
            IsActive = true,
        };
        _db.TenantClients.Add(tenantClient);
        await _db.SaveChangesAsync();

        return tenantClient;
    }

    public async Task<TenantClientResponse> UpdateClientAsync(Guid tenantId, Guid clientId, UpdateTenantClientRequest request)
    {
        var tenantClient = await _db.TenantClients
            .FirstOrDefaultAsync(c => c.Id == clientId && c.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Client OAuth non trouvé");

        var app = await _applicationManager.FindByIdAsync(tenantClient.ApplicationId)
            ?? throw new InvalidOperationException("Application OpenIddict introuvable");

        UpdateClientProperties(tenantClient, request);

        var descriptor = new OpenIddictApplicationDescriptor();
        await _applicationManager.PopulateAsync(descriptor, app);

        var needUpdate = false;
        needUpdate |= UpdateClientUris(tenantClient, descriptor, request);
        needUpdate |= UpdateClientPermissions(descriptor, request, tenantClient);

        tenantClient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (needUpdate)
            await _applicationManager.UpdateAsync(app, descriptor);
        return MapToResponse(tenantClient);
    }

    private static void UpdateClientProperties(TenantClient tenantClient, UpdateTenantClientRequest request)
    {
        if (request.ClientName != null) tenantClient.ClientName = request.ClientName;
        if (request.Description != null) tenantClient.Description = request.Description;
        if (request.IsActive.HasValue) tenantClient.IsActive = request.IsActive.Value;
    }

    public async Task DeleteClientAsync(Guid tenantId, Guid clientId)
    {
        var tenantClient = await _db.TenantClients
            .FirstOrDefaultAsync(c => c.Id == clientId && c.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Client OAuth non trouvé");
        var app = await _applicationManager.FindByIdAsync(tenantClient.ApplicationId);
        if (app != null)
            await _applicationManager.DeleteAsync(app);
        _db.TenantClients.Remove(tenantClient);
        await _db.SaveChangesAsync();
    }

    private static TenantClientResponse MapToResponse(TenantClient c) =>
        new(c.Id, c.TenantId, c.ClientId, c.ClientName, c.Description,
            DeserializeArray(c.RedirectUris), DeserializeArray(c.PostLogoutRedirectUris),
            DeserializeArray(c.AllowedGrantTypes), c.IsActive, c.CreatedAt);

    private static List<string> DeserializeArray(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? [];

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(bytes);
    }
}
