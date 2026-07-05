using DotnetNiger.Common.Auth;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using DotnetNiger.Common.Auth.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DotnetNiger.Common.Email;

namespace DotnetNiger.Identity.Application.Services;

internal static class CodeGenerator
{
    private static readonly char[] CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public static string Generate()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(6);
        var code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = CodeChars[bytes[i] % CodeChars.Length];
        return new string(code);
    }
}

public partial class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TenantContext _tenantContext;
    private readonly IdentityDbContext _db;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IPermissionService _permissionService;
    private readonly AccountService _accountService;

    public AuthService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TenantContext tenantContext, IdentityDbContext db,
        IEmailSender<ApplicationUser> emailSender,
        IOptions<SmtpOptions> smtp,
        IPermissionService permissionService,
        AccountService accountService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tenantContext = tenantContext;
        _db = db;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _permissionService = permissionService;
        _accountService = accountService;
    }

    public async Task<(ApplicationUser user, IList<string> roles)> ValidateCredentialsAsync(
        string email, string password, Guid? tenantId = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect");

        if (tenantId.HasValue && user.TenantId != tenantId.Value)
            throw new UnauthorizedAccessException("Utilisateur non trouvé dans ce tenant");

        if (!await _userManager.IsEmailConfirmedAsync(user))
            throw new UnauthorizedAccessException("Email non confirmé");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);
        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Compte temporairement verrouillé");
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect");

        var roles = await _userManager.GetRolesAsync(user);
        _tenantContext.TenantId = user.TenantId;
        return (user, roles);
    }

    public async Task<UserInfoResponse> LoginAsync(string email, string password, Guid? tenantId, bool rememberMe, string ipAddress, string userAgent)
    {
        ApplicationUser? user = null;
        try
        {
            (user, var roles) = await ValidateCredentialsAsync(email, password, tenantId);
            await RecordLoginAsync(user.Id, ipAddress, userAgent, true);
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            return new UserInfoResponse(
                user.Id, user.Email!, user.FirstName, user.LastName, user.AvatarUrl,
                user.TenantId, user.IsActive, roles, permissions, rememberMe);
        }
        catch (UnauthorizedAccessException ex)
        {
            if (user != null)
                await RecordLoginAsync(user.Id, ipAddress, userAgent, false, failureReason: ex.Message);
            throw;
        }
    }

    public async Task<(ApplicationUser user, IList<string> roles)> HandleExternalLoginAsync(string provider)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            throw new InvalidOperationException("Erreur lors du login externe");

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
            return await HandleExistingExternalLoginAsync(info);

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Email requis pour le login externe");

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return await LinkToExistingAccountAsync(existingUser, info);

        return await CreateUserFromExternalLoginAsync(info);
    }

    private async Task<(ApplicationUser user, IList<string> roles)> HandleExistingExternalLoginAsync(ExternalLoginInfo info)
    {
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        var roles = await _userManager.GetRolesAsync(user!);
        _tenantContext.TenantId = user!.TenantId;
        return (user, roles)!;
    }

    private async Task<(ApplicationUser user, IList<string> roles)> LinkToExistingAccountAsync(ApplicationUser existingUser, ExternalLoginInfo info)
    {
        await _userManager.AddLoginAsync(existingUser, info);
        existingUser.EmailConfirmed = true;
        await _userManager.UpdateAsync(existingUser);
        var roles = await _userManager.GetRolesAsync(existingUser);
        _tenantContext.TenantId = existingUser.TenantId;
        return (existingUser, roles);
    }

    private async Task<(ApplicationUser user, IList<string> roles)> CreateUserFromExternalLoginAsync(ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? throw new InvalidOperationException("Email requis pour le login externe");

        var tenant = await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
            throw new InvalidOperationException("Aucun tenant configuré");

        var newUser = new ApplicationUser
        {
            UserName = email, Email = email, EmailConfirmed = true,
            TenantId = tenant.Id,
            FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value,
            LastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value
        };
        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
            throw new InvalidOperationException("Erreur création utilisateur");

        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "User");
        _tenantContext.TenantId = newUser.TenantId;
        return (newUser, new List<string> { "User" });
    }

    public async Task RecordLoginAsync(Guid userId, string ipAddress, string userAgent, bool success, string? failureReason = null)
    {
        var entry = new LoginHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason
        };
        _db.LoginHistories.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task<object> GetLoginHistoryAsync(Guid userId, int page, int pageSize)
    {
        var query = _db.LoginHistories.Where(h => h.UserId == userId);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new
            {
                h.Id, h.IpAddress, h.UserAgent, h.Success, h.FailureReason, h.CreatedAt
            })
            .ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
