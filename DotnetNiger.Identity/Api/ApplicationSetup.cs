using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using DotnetNiger.Identity.Api.Middleware;
using DotnetNiger.Identity.Infrastructure;
using Serilog;

namespace DotnetNiger.Identity.Api;

public static class ApplicationSetup
{
    public static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ApplicationSetup).Assembly);
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddIdentityInfrastructure(builder.Configuration, builder.Environment);
        builder.Services.AddIdentityServices();
        builder.Services.AddRateLimitingPolicies(builder.Configuration);
        builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformer>();
        builder.Services.AddApiVersioningWithSwagger();

        return builder;
    }

    public static WebApplication ConfigureApp(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseSerilogRequestLogging();
        app.UseCors("AllowAll");
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantResolutionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DotnetNiger Identity v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            service = "DotnetNiger.Identity",
            timestamp = DateTime.UtcNow
        }));

        app.MapGet("/health/ready", async ([FromServices] IdentityDbContext idCtx) =>
        {
            try
            {
                await idCtx.Database.CanConnectAsync();

                return Results.Ok(new
                {
                    status = "Ready",
                    service = "DotnetNiger.Identity",
                    timestamp = DateTime.UtcNow,
                    checks = new
                    {
                        database = "connected"
                    }
                });
            }
            catch
            {
                return Results.StatusCode(503);
            }
        });

        app.MapGet("/health/downstream", async ([FromServices] IdentityDbContext idCtx,
            [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            try
            {
                await idCtx.Database.CanConnectAsync();

                return Results.Ok(new
                {
                    status = "Healthy",
                    service = "DotnetNiger.Identity",
                    timestamp = DateTime.UtcNow,
                    checks = new
                    {
                        database = "connected",
                        downstream = "not_checked"
                    }
                });
            }
            catch
            {
                return Results.StatusCode(503);
            }
        });

        app.MapControllers();
        app.MapRazorPages();

        return app;
    }

    public static async Task SeedDataAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.UserManager<DotnetNiger.Identity.Domain.Entities.ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<
            Microsoft.AspNetCore.Identity.RoleManager<DotnetNiger.Identity.Domain.Entities.ApplicationRole>>();

        var adminPassword = app.Configuration["Admin:DefaultPassword"] ?? "Admin@123456";
        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        await DbSeeder.SeedAsync(db, userManager, roleManager, tenantContext, adminPassword, appManager);
    }
}
