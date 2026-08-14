using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Application.Services.Admin;
using DotnetNiger.Api.Application.Services.Settings;
using DotnetNiger.Api.Application.Services.Users;
using DotnetNiger.Api.Application.Services.Support;
using DotnetNiger.Api.Application.Services.Categories;
using DotnetNiger.Api.Application.Services.Members;
using DotnetNiger.Api.Application.Services.Posts;
using DotnetNiger.Api.Application.Services.Events;
using DotnetNiger.Api.Application.Services.Resources;
using DotnetNiger.Api.Application.Services.Tags;
using DotnetNiger.Api.Application.Services.Comments;
using DotnetNiger.Api.Application.Services.Contact;
using DotnetNiger.Api.Application.Services.Newsletter;
using DotnetNiger.Api.Application.Services.Partners;
using DotnetNiger.Api.Application.Services.Projects;
using DotnetNiger.Api.Application.Services.Search;
using DotnetNiger.Api.Application.Services.Certificates;
using DotnetNiger.Api.Application.Services.ImageProcessing;
using DotnetNiger.Api.Application.Services.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api;

public static class ServiceRegistration
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddScoped<TokenService>();
        services.AddScoped<AccountService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<DashboardService>();

        services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();
        services.AddScoped<IEmailService, EmailSender>();
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
