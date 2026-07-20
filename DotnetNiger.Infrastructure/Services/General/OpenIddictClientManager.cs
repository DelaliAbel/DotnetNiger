using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Data;

namespace DotnetNiger.Infrastructure.Services.General;

public partial class OpenIddictClientManager
{
    private readonly DotnetNigerDbContext _db;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public OpenIddictClientManager(DotnetNigerDbContext db, IOpenIddictApplicationManager applicationManager)
    {
        _db = db;
        _applicationManager = applicationManager;
    }

    public async Task<OAuthClientCreatedResponse> CreateClientAsync(CreateOAuthClientRequest request)
    {
        var clientId = $"app_{Guid.NewGuid():N}";
        var clientSecret = GenerateSecret();

        var descriptor = BuildClientDescriptor(request, clientId, clientSecret);
        if (await _applicationManager.FindByClientIdAsync(clientId) != null)
            throw new InvalidOperationException($"Un client avec l'identifiant {clientId} existe déjà.");

        var app = await _applicationManager.CreateAsync(descriptor)
            ?? throw new InvalidOperationException("Échec de la création de l'application OpenIddict.");

        var oAuthClient = await SaveOAuthClientAsync(request, clientId, clientSecret, app);

        return new OAuthClientCreatedResponse(MapToResponse(oAuthClient), clientSecret);
    }

    private async Task<OAuthClient> SaveOAuthClientAsync(CreateOAuthClientRequest request, string clientId, string clientSecret, object app)
    {
        var grantTypes = OAuthClientService.ParseJsonArrayOrDefault(request.AllowedGrantTypes,
            ["authorization_code", "password", "refresh_token", "client_credentials"]);
        var redirectUris = OAuthClientService.ParseJsonArray(request.RedirectUris);
        var postLogoutUris = OAuthClientService.ParseJsonArray(request.PostLogoutRedirectUris);

        var oAuthClient = new OAuthClient
        {
            Id = Guid.NewGuid(),
            ApplicationId = (await _applicationManager.GetIdAsync(app))!,
            ClientId = clientId,
            ClientSecretHash = HashSecret(clientSecret),
            ClientName = request.ClientName, Description = request.Description,
            RedirectUris = JsonSerializer.Serialize(redirectUris),
            PostLogoutRedirectUris = JsonSerializer.Serialize(postLogoutUris),
            AllowedGrantTypes = JsonSerializer.Serialize(grantTypes),
            IsActive = true,
        };
        _db.OAuthClients.Add(oAuthClient);
        await _db.SaveChangesAsync();

        return oAuthClient;
    }

    public async Task<OAuthClientResponse> UpdateClientAsync(Guid clientId, UpdateOAuthClientRequest request)
    {
        var oAuthClient = await _db.OAuthClients
            .FirstOrDefaultAsync(c => c.Id == clientId)
            ?? throw new KeyNotFoundException("Client OAuth non trouvé");

        var app = await _applicationManager.FindByIdAsync(oAuthClient.ApplicationId)
            ?? throw new InvalidOperationException("Application OpenIddict introuvable");

        UpdateClientProperties(oAuthClient, request);

        var descriptor = new OpenIddictApplicationDescriptor();
        await _applicationManager.PopulateAsync(descriptor, app);

        var needUpdate = false;
        needUpdate |= UpdateClientUris(oAuthClient, descriptor, request);
        needUpdate |= UpdateClientPermissions(descriptor, request, oAuthClient);

        oAuthClient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (needUpdate)
            await _applicationManager.UpdateAsync(app, descriptor);
        return MapToResponse(oAuthClient);
    }

    private static void UpdateClientProperties(OAuthClient oAuthClient, UpdateOAuthClientRequest request)
    {
        if (request.ClientName != null) oAuthClient.ClientName = request.ClientName;
        if (request.Description != null) oAuthClient.Description = request.Description;
        if (request.IsActive.HasValue) oAuthClient.IsActive = request.IsActive.Value;
    }

    public async Task DeleteClientAsync(Guid clientId)
    {
        var oAuthClient = await _db.OAuthClients
            .FirstOrDefaultAsync(c => c.Id == clientId)
            ?? throw new KeyNotFoundException("Client OAuth non trouvé");
        var app = await _applicationManager.FindByIdAsync(oAuthClient.ApplicationId);
        if (app != null)
            await _applicationManager.DeleteAsync(app);
        _db.OAuthClients.Remove(oAuthClient);
        await _db.SaveChangesAsync();
    }

    private static OAuthClientResponse MapToResponse(OAuthClient c) =>
        new(c.Id, c.ClientId, c.ClientName, c.Description,
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
