using DotnetNiger.Community.Api.Services;
using DotnetNiger.Community.Application.Abstractions.Persistence;
using DotnetNiger.Community.Application.Mappers;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Application.Services.Interfaces;
using DotnetNiger.Community.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Community.Api.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services Community
/// Configure les dépendances pour IoC container
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddCommunityApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITeamMemberService, TeamMemberService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<ISlugGenerator, SlugGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICommunityRequestMapper, CommunityRequestMapper>();
        services.AddScoped<INewsletterService, NewsletterService>();

        return services;
    }

    public static IServiceCollection AddCommunityRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPostPersistence, PostRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectPersistence, ProjectRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IResourcePersistence, ResourceRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
        services.AddScoped<IEventPersistence, EventRepository>();
        services.AddScoped<IEventRegistrationPersistence, EventRegistrationRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryPersistence, CategoryRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentPersistence, CommentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITagPersistence, TagRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IPartnerPersistence, PartnerRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<ITeamMemberPersistence, TeamMemberRepository>();
        services.AddScoped<INewsletterSubscriptionRepository, NewsletterSubscriptionRepository>();
        services.AddScoped<INewsletterSubscriptionPersistence, NewsletterSubscriptionRepository>();

        return services;
    }
}
