using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Services.General;
using DotnetNiger.Api.Services.Admin;
using DotnetNiger.Api.Data;
using DotnetNiger.Api.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api;

/// <summary>
/// Enregistre tous les services métier Identity dans le DI container.
/// Plus de dépendance OpenIddict — tout est natif Microsoft Identity + JWT.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Services auth et identity (scoped).</summary>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        // --- Auth / Identity ---
        services.AddScoped<TokenService>();
        services.AddScoped<AuthService>();
        services.AddScoped<AccountService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<DashboardService>();

        // --- Email ---
        services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();
        services.AddScoped<EmailSender>();

        // --- Support ---
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // --- Contenu ---
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMemberDirectoryService, MemberDirectoryService>();
        services.AddScoped<IPostCommandService, PostCommandService>();
        services.AddScoped<IPostQueryService, PostQueryService>();
        services.AddScoped<IPostModerationService, PostModerationService>();
        services.AddScoped<IEventCommandService, EventCommandService>();
        services.AddScoped<IEventQueryService, EventQueryService>();
        services.AddScoped<IEventModerationService, EventModerationService>();
        services.AddScoped<IEventRegistrationService, EventRegistrationService>();
        services.AddScoped<IResourceCommandService, ResourceCommandService>();
        services.AddScoped<IResourceQueryService, ResourceQueryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<INewsletterService, NewsletterService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();

        // --- Background ---
        services.AddHostedService<DeletionProcessorService>();

        return services;
    }
}
