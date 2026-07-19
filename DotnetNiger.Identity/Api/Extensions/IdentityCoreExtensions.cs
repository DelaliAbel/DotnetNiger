using DotnetNiger.Common.Email;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore;

namespace DotnetNiger.Identity.Api.Extensions;

public static class IdentityCoreExtensions
{
    /// <summary>Configure le DbContext, Identity (UserManager/RoleManager), cookies, MemoryCache et SmtpOptions.</summary>
    public static IServiceCollection AddIdentityCore(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddMemoryCache();
        services.AddDbContext<IdentityDbContext>(options =>
        {
            var connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required");
            options.UseSqlServer(connStr, x => x.MigrationsAssembly("DotnetNiger.Identity"));
            options.UseOpenIddict();
        });

        services.Configure<SmtpOptions>(config.GetSection("Smtp"));

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
