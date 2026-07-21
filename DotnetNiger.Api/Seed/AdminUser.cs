using DotnetNiger.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace DotnetNiger.Api.Seed;

public static class AdminUser
{
    public const string Email = "admin@dotnetniger.org";
    public const string Password = "Admin@123456";
    public const string Role = "SuperAdmin";

    public static Guid? AdminId { get; private set; }

    public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(Email) != null)
            return;

        var admin = new ApplicationUser
        {
            UserName = Email,
            Email = Email,
            FirstName = "Admin",
            LastName = "DotnetNiger",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, Password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Role);
            AdminId = admin.Id;
        }
    }
}
