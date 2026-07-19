using DotnetNiger.Client.Services;
using DotnetNiger.Client.Services.Api;
using DotnetNiger.Client.Services.Auth;
using DotnetNiger.Client.Services.Browser;
using DotnetNiger.Client.Services.Contracts;
using DotnetNiger.Client.Services.Mock;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DotnetNiger.Client;

public static class ServiceCollectionExtensions
{
    public static void AddAppServices(
        this IServiceCollection services,
        string apiBaseUrl,
        IConfiguration configuration)
    {
        // Auth
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));
        });
        services.AddScoped<CustomAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<CustomAuthStateProvider>());
        services.AddScoped<ILocalStorageService, JsLocalStorageService>();

        // Theme
        services.AddScoped<ThemeService>();

        // Preview
        services.AddSingleton<PreviewStateService>();

        // Services applicatifs — Mock uniquement en DEBUG, jamais en Release/Production
#if DEBUG
        var useMock = configuration.GetValue<bool>("UseMockServices");
        if (useMock)
        {
            services.AddScoped<IPermissionService, MockPermissionService>();
            services.AddScoped<IToastService, ToastService>();
            services.AddScoped<IUploadService, MockUploadService>();
            services.AddScoped<IAuthService, MockAuthService>();
            services.AddScoped<IUserService, MockUserService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IRegistrationService, MockRegistrationService>();
            services.AddScoped<IUserStateService, UserStateService>();
            services.AddScoped<IProjectService, MockProjectService>();
            services.AddScoped<IPartnerService, MockPartnerService>();
            services.AddScoped<INewsletterService, MockNewsletterService>();
            services.AddScoped<IMemberDirectoryService, MockMemberDirectoryService>();
            services.AddScoped<ISearchService, MockSearchService>();
            services.AddScoped<IContactService, MockContactService>();
        }
        else
#endif
        {
            services.AddScoped<IToastService, ToastService>();
            services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
            services.AddScoped<IUserService>(sp =>
                new ApiUserService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IPostService>(sp =>
                new ApiPostService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IEventService>(sp =>
                new ApiEventService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IResourceService>(sp =>
                new ApiResourceService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IProfileService>(sp =>
                new ApiProfileService(
                    CreateGatewayHttpClient(
                        apiBaseUrl,
                        sp.GetRequiredService<ClientIdentifierProvider>(),
                        sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<ICommentService>(sp =>
                new ApiCommentService(
                    CreateGatewayHttpClient(
                        apiBaseUrl,
                        sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>()),
                    sp.GetRequiredService<CustomAuthStateProvider>()));
            services.AddScoped<IRegistrationService>(sp =>
                new ApiRegistrationService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<INotificationService>(sp =>
                new ApiNotificationService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IContactService>(sp =>
                new ApiContactService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IProjectService>(sp =>
                new ApiProjectService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IPartnerService>(sp =>
                new ApiPartnerService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<INewsletterService>(sp =>
                new ApiNewsletterService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IMemberDirectoryService>(sp =>
                new ApiMemberDirectoryService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<ISearchService>(sp =>
                new ApiSearchService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IUserStateService, UserStateService>();
            services.AddScoped<IUploadService>(sp =>
                new ApiUploadService(
                    CreateGatewayHttpClient(
                        apiBaseUrl,
                        sp.GetRequiredService<ClientIdentifierProvider>(),
                        sp.GetRequiredService<CustomAuthStateProvider>(),
                        sp,
                        sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>()),
                    sp.GetRequiredService<ILogger<ApiUploadService>>()));
            services.AddScoped<ICategoryService>(sp =>
                new ApiCategoryService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<ITagService>(sp =>
                new ApiTagService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IStatsService>(sp =>
                new ApiStatsService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<ISettingsService>(sp =>
                new ApiSettingsService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<ICertificateAdminService>(sp =>
                new ApiCertificateAdminService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
            services.AddScoped<IPermissionService>(sp =>
                new PermissionService(CreateGatewayHttpClient(
                    apiBaseUrl,
                    sp.GetRequiredService<ClientIdentifierProvider>(),
                    sp.GetRequiredService<CustomAuthStateProvider>(),
                    sp,
                    sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>())));
        }
    }

    internal static HttpClient CreateGatewayHttpClient(
        string baseUrl,
        ClientIdentifierProvider clientIdentifierProvider,
        CustomAuthStateProvider authStateProvider,
        IServiceProvider serviceProvider,
        ILogger<ClientIdHeaderHandler> logger)
    {
        var navigationManager = serviceProvider.GetRequiredService<NavigationManager>();
        var headerHandler = new ClientIdHeaderHandler(clientIdentifierProvider, authStateProvider, serviceProvider, logger, navigationManager)
        {
            InnerHandler = new HttpClientHandler()
        };

        return new HttpClient(headerHandler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }
}
