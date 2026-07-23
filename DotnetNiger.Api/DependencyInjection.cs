using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Interfaces;
using DotnetNiger.Api.Data;
using DotnetNiger.Api.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api;

public static class DependencyInjection
{
    /// <summary>Enregistre tous les services métier Identity (scoped) dans le DI container.</summary>
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<AccountService>();
        services.AddScoped<TokenService>();
        services.AddScoped<OidcService>();
        services.AddScoped<OpenIddictManagementService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<OpenIddictClientManager>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();
        services.AddScoped<EmailSender>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

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

        services.AddHostedService<DeletionProcessorService>();

        return services;
    }
}
