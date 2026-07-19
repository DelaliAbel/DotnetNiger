using DotnetNiger.Client;
using DotnetNiger.Client.Services;
using DotnetNiger.Client.Services.Auth;
using DotnetNiger.Client.Services.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

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

builder.Services.AddScoped<AuthService>(sp => new AuthService(
    ServiceCollectionExtensions.CreateGatewayHttpClient(
        apiBaseUrl,
        sp.GetRequiredService<ClientIdentifierProvider>(),
        sp.GetRequiredService<CustomAuthStateProvider>(),
        sp,
        sp.GetRequiredService<ILogger<ClientIdHeaderHandler>>()),
    sp.GetRequiredService<CustomAuthStateProvider>(),
    sp.GetRequiredService<IUserStateService>(),
    sp.GetRequiredService<IPermissionService>(),
    clientId
));

builder.Services.AddAppServices(apiBaseUrl, builder.Configuration);

await builder.Build().RunAsync();
