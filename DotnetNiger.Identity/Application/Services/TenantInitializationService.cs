using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static DotnetNiger.Common.Constants.RoleConstants;
using DotnetNiger.Common.Exceptions;
using DotnetNiger.Common.Auth.Responses;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using OpenIddict.Abstractions;

namespace DotnetNiger.Identity.Application.Services;

public class TenantInitializationService
{
    private readonly IdentityDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITenantApiKeyService _apiKeyService;
    private readonly OpenIddictManagementService _oidcManagement;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly string _adminPassword;

    public TenantInitializationService(IdentityDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITenantApiKeyService apiKeyService,
        OpenIddictManagementService oidcManagement,
        IOpenIddictApplicationManager applicationManager,
        IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _apiKeyService = apiKeyService;
        _oidcManagement = oidcManagement;
        _applicationManager = applicationManager;
        _adminPassword = configuration["Admin:DefaultPassword"]
            ?? throw new InvalidOperationException("Admin:DefaultPassword must be set via user-secrets or environment variable");
    }

    public async Task<TenantResponse> CreateWithDefaultsAsync(CreateTenantRequest request)
    {
        if (await _db.Tenants.AnyAsync(t => t.Slug == request.Slug.ToLowerInvariant()))
            throw new SlugAlreadyExistsException(request.Slug);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(), Name = request.Name,
            Slug = request.Slug.ToLowerInvariant(),
            Description = request.Description, IsActive = true
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        await CreateRolesAsync(tenant.Id, tenant.Name);
        await CreateAdminUserAsync(tenant, $"admin@{tenant.Slug}.dotnetniger.com");
        return MapToResponse(tenant);
    }

    public async Task<RegisterTenantResponse> RegisterTenantAsync(RegisterTenantRequest request)
    {
        if (await _db.Tenants.AnyAsync(t => t.Slug == request.Slug.ToLowerInvariant()))
            throw new SlugAlreadyExistsException(request.Slug);
        if (await _userManager.FindByEmailAsync(request.AdminEmail) != null)
            throw new EmailAlreadyExistsException(request.AdminEmail);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(), Name = request.CompanyName,
            Slug = request.Slug.ToLowerInvariant(), IsActive = true
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        await CreateRolesAsync(tenant.Id, tenant.Name);
        var adminUser = await CreateAdminUserAsync(tenant, request.AdminEmail, request.AdminFirstName, request.AdminLastName, request.AdminPassword);
        var (clientId, clientSecret) = await _oidcManagement.CreateDefaultClientAsync(_applicationManager, tenant);
        var apiKey = await _apiKeyService.CreateApiKeyAsync(tenant.Id,
            new CreateTenantApiKeyRequest($"{tenant.Name} — Clé API par défaut", "[\"api\"]", null));
        return new RegisterTenantResponse(
            tenant.Id, tenant.Name, tenant.Slug, request.AdminEmail,
            clientId, clientSecret, apiKey.Key.Id, apiKey.PrivateKey);
    }

    private async Task CreateRolesAsync(Guid tenantId, string tenantName)
    {
        await _roleManager.CreateAsync(new ApplicationRole
        {
            Name = Admin, NormalizedName = "ADMIN",
            TenantId = tenantId, Description = $"Administrateur de {tenantName}"
        });
        await _roleManager.CreateAsync(new ApplicationRole
        {
            Name = User, NormalizedName = "USER",
            TenantId = tenantId, Description = "Utilisateur standard"
        });
    }

    private async Task<ApplicationUser> CreateAdminUserAsync(Tenant tenant, string email,
        string? firstName = "Admin", string? lastName = null, string? password = null)
    {
        var user = new ApplicationUser
        {
            UserName = email, Email = email,
            FirstName = firstName!, LastName = lastName ?? tenant.Name,
            TenantId = tenant.Id, IsActive = true, EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, password ?? _adminPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Échec création admin : {string.Join(", ", result.Errors.Select(e => e.Description))}");
        await _userManager.AddToRoleAsync(user, Admin);
        return user;
    }

    private static TenantResponse MapToResponse(Tenant t) =>
        new(t.Id, t.Name, t.Slug, t.Description, t.IsActive, t.CreatedAt);
}
