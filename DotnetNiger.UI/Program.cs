using DotnetNiger.UI;
using DotnetNiger.UI.Services.Browser;
using DotnetNiger.UI.Services.Auth;
using DotnetNiger.UI.Services.Api;
using DotnetNiger.UI.Services.App;
using DotnetNiger.UI.Configuration;
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

// Client HTTP dédié pour AuthService — configurez ApiBaseUrl dans wwwroot/appsettings.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
var clientId = builder.Configuration["ClientId"] ?? "web-ui";
builder.Services.AddScoped<ClientIdentifierProvider>();
builder.Services.AddSingleton(new ApiBaseUrlProvider(apiBaseUrl));

// Client HTTP Gateway partagé — BaseAddress pointe vers l'API
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
    builder.Services.AddScoped<IConfirmService, ConfirmService>();
    builder.Services.AddScoped<IUploadService, MockUploadService>();
    builder.Services.AddScoped<IAuthService, MockAuthService>();
    builder.Services.AddScoped<IUserService, MockUserService>();
    builder.Services.AddScoped<IPostService, MockPostService>();
    builder.Services.AddScoped<IEventService, MockEventService>();
    builder.Services.AddScoped<INotificationService, MockNotificationService>();
    builder.Services.AddScoped<IResourceService, MockResourceService>();
    builder.Services.AddScoped<IProfileService, MockProfileService>();
    builder.Services.AddScoped<ICommentService, MockCommentService>();
    builder.Services.AddScoped<IRegistrationService, MockRegistrationService>();
    builder.Services.AddScoped<IUserStateService, UserStateService>();
    builder.Services.AddScoped<IProjectService, MockProjectService>();
    builder.Services.AddScoped<IPartnerService, MockPartnerService>();
    builder.Services.AddScoped<INewsletterService, MockNewsletterService>();
    builder.Services.AddScoped<IMemberDirectoryService, MockMemberDirectoryService>();
    builder.Services.AddScoped<ISearchService, MockSearchService>();
    builder.Services.AddScoped<IContactService, MockContactService>();
    builder.Services.AddScoped<ICategoryService, MockCategoryService>();
    builder.Services.AddScoped<ITagService, MockTagService>();
    builder.Services.AddScoped<IStatsService, MockStatsService>();
    builder.Services.AddScoped<ISettingsService, MockSettingsService>();
    builder.Services.AddScoped<ICertificateAdminService, MockCertificateAdminService>();
}
else
#endif
{
    builder.Services.AddScoped<IConfirmService, ConfirmService>();
    builder.Services.AddScoped<IToastService, ToastService>();
    builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
    builder.Services.AddScoped<IUserService>(sp => new ApiUserService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiUserService>>()));
    builder.Services.AddScoped<IPostService>(sp => new ApiPostService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiPostService>>()));
    builder.Services.AddScoped<IEventService>(sp => new ApiEventService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiEventService>>()));
    builder.Services.AddScoped<IResourceService>(sp => new ApiResourceService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiResourceService>>()));
    builder.Services.AddScoped<IProfileService>(sp => new ApiProfileService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiProfileService>>()));
    builder.Services.AddScoped<ICommentService>(sp => new ApiCommentService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiCommentService>>(), sp.GetRequiredService<CustomAuthStateProvider>()));
    builder.Services.AddScoped<IRegistrationService>(sp => new ApiRegistrationService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiRegistrationService>>()));
    builder.Services.AddScoped<INotificationService>(sp => new ApiNotificationService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiNotificationService>>()));
    builder.Services.AddScoped<IContactService>(sp => new ApiContactService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiContactService>>()));
    builder.Services.AddScoped<IProjectService>(sp => new ApiProjectService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiProjectService>>()));
    builder.Services.AddScoped<IPartnerService>(sp => new ApiPartnerService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiPartnerService>>()));
    builder.Services.AddScoped<INewsletterService>(sp => new ApiNewsletterService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiNewsletterService>>()));
    builder.Services.AddScoped<IMemberDirectoryService>(sp => new ApiMemberDirectoryService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiMemberDirectoryService>>()));
    builder.Services.AddScoped<ISearchService>(sp => new ApiSearchService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiSearchService>>()));
    builder.Services.AddScoped<IUserStateService, UserStateService>();
    builder.Services.AddScoped<IUploadService>(sp => new ApiUploadService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiUploadService>>(), sp.GetRequiredService<ApiBaseUrlProvider>()));
    builder.Services.AddScoped<ICategoryService>(sp => new ApiCategoryService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiCategoryService>>()));
    builder.Services.AddScoped<ITagService>(sp => new ApiTagService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiTagService>>()));
    builder.Services.AddScoped<IStatsService>(sp => new ApiStatsService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiStatsService>>()));
    builder.Services.AddScoped<ISettingsService>(sp => new ApiSettingsService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiSettingsService>>()));
    builder.Services.AddScoped<ICertificateAdminService>(sp => new ApiCertificateAdminService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<ApiCertificateAdminService>>()));
    builder.Services.AddScoped<IPermissionService>(sp => new PermissionService(sp.GetRequiredService<CustomAuthStateProvider>()));
}

await builder.Build().RunAsync();
