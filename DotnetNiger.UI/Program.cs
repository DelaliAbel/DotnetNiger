using DotnetNiger.UI;
using DotnetNiger.UI.Services.Browser;
using DotnetNiger.UI.Services.Auth;
using DotnetNiger.UI.Services.Api;
using DotnetNiger.UI.Services;
using DotnetNiger.UI.Services.Mock;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Client HTTP pour les ressources statiques de l'application
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Client HTTP dédié pour AuthService — configurez ApiBaseUrl dans wwwroot/appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
var clientId = builder.Configuration["ClientId"] ?? "web-ui";
builder.Services.AddScoped<ClientIdentifierProvider>();
builder.Services.AddSingleton(new ApiBaseUrlProvider(apiBaseUrl));

// Client HTTP Gateway partagé
builder.Services.AddTransient<ClientIdHeaderHandler>();
builder.Services.AddHttpClient("DotnetNiger.Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<ClientIdHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DotnetNiger.Api"));


builder.Services.AddScoped<AuthService>(sp => new AuthService(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("DotnetNiger.Api"),
    sp.GetRequiredService<CustomAuthStateProvider>(),
    sp.GetRequiredService<IUserStateService>(),
    sp.GetRequiredService<IPermissionService>(),
    clientId
));

// Auth
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
});
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<ILocalStorageService, JsLocalStorageService>();

// Theme
builder.Services.AddScoped<ThemeService>();

// Preview
builder.Services.AddSingleton<PreviewStateService>();

// Services applicatifs — Mock uniquement en DEBUG, jamais en Release/Production
#if DEBUG
var useMock = builder.Configuration.GetValue<bool>("UseMockServices");
if (useMock)
{
    builder.Services.AddScoped<IPermissionService, MockPermissionService>();
    builder.Services.AddScoped<IToastService, ToastService>();
    builder.Services.AddScoped<IUploadService, MockUploadService>();
    builder.Services.AddScoped<IAuthService, MockAuthService>();
    builder.Services.AddScoped<IUserService, MockUserService>();
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddScoped<IEventService, EventService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IResourceService, ResourceService>();
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IRegistrationService, MockRegistrationService>();
    builder.Services.AddScoped<IUserStateService, UserStateService>();
    builder.Services.AddScoped<IProjectService, MockProjectService>();
    builder.Services.AddScoped<IPartnerService, MockPartnerService>();
    builder.Services.AddScoped<INewsletterService, MockNewsletterService>();
    builder.Services.AddScoped<IMemberDirectoryService, MockMemberDirectoryService>();
    builder.Services.AddScoped<ISearchService, MockSearchService>();
    builder.Services.AddScoped<IContactService, MockContactService>();
}
else
#endif
{
    builder.Services.AddScoped<IToastService, ToastService>();
    builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
    builder.Services.AddScoped<IUserService>(sp => new ApiUserService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IPostService>(sp => new ApiPostService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IEventService>(sp => new ApiEventService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IResourceService>(sp => new ApiResourceService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IProfileService>(sp => new ApiProfileService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<ICommentService>(sp => new ApiCommentService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<CustomAuthStateProvider>()));
    builder.Services.AddScoped<IRegistrationService>(sp => new ApiRegistrationService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<INotificationService>(sp => new ApiNotificationService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IContactService>(sp => new ApiContactService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IProjectService>(sp => new ApiProjectService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IPartnerService>(sp => new ApiPartnerService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<INewsletterService>(sp => new ApiNewsletterService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IMemberDirectoryService>(sp => new ApiMemberDirectoryService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<ISearchService>(sp => new ApiSearchService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IUserStateService, UserStateService>();
    builder.Services.AddScoped<IUploadService>(sp => new ApiUploadService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiUploadService>>()));
    builder.Services.AddScoped<ICategoryService>(sp => new ApiCategoryService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<ITagService>(sp => new ApiTagService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IStatsService>(sp => new ApiStatsService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<ISettingsService>(sp => new ApiSettingsService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<ICertificateAdminService>(sp => new ApiCertificateAdminService(sp.GetRequiredService<HttpClient>()));
    builder.Services.AddScoped<IPermissionService>(sp => new PermissionService(sp.GetRequiredService<HttpClient>()));
}

await builder.Build().RunAsync();
