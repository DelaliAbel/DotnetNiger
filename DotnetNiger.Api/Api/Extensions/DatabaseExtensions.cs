using System.Security.Claims;
using DotnetNiger.Api.Domain.Entities;
using DotnetNiger.Api.Infrastructure.Data;
using DotnetNiger.Api.Infrastructure.Email;
using DotnetNiger.Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetNiger.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseWithIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DotnetNigerDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
        })
        .AddEntityFrameworkStores<DotnetNigerDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        return services;
    }
}
