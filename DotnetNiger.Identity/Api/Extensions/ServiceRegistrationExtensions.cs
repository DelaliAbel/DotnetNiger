using DotnetNiger.Common.Auth;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Identity.Api.Extensions;

public static class ServiceRegistrationExtensions
{
    /// <summary>Enregistre tous les services métier Identity (scoped) dans le DI container.</summary>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();
        services.AddScoped<TenantResolutionService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        services.AddScoped<AccountService>();
        services.AddScoped<TokenService>();
        services.AddScoped<TwoFactorService>();
        services.AddScoped<OidcService>();
        services.AddScoped<OpenIddictManagementService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<TenantService>();
        services.AddScoped<TenantInitializationService>();
        services.AddScoped<TenantClientService>();
        services.AddScoped<OpenIddictClientManager>();
        services.AddScoped<ITenantApiKeyService, TenantApiKeyService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();
        services.AddScoped<EmailSender>();
        services.AddScoped<IExternalServiceService, ExternalServiceService>();
        services.AddScoped<GdprService>();
        services.AddScoped<GdprExportService>();

        return services;
    }
}
