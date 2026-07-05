using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using DotnetNiger.Common.Constants;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Api.Models;
using DotnetNiger.Identity.Application.Services;

namespace DotnetNiger.Identity.Pages.Account;

[EnableRateLimiting("Auth")]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AuthService _authService;
    private readonly ILogger<LoginModel> _logger;
    private readonly IMemoryCache _cache;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        AuthService authService,
        ILogger<LoginModel> logger,
        IMemoryCache cache)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _authService = authService;
        _logger = logger;
        _cache = cache;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }

    public IList<AuthenticationScheme> ExternalProviders { get; set; } = new List<AuthenticationScheme>();

    public async Task OnGetAsync()
    {
        ReturnUrl ??= "/";
        if (Request.Query.TryGetValue("error", out var errorVal) && !string.IsNullOrEmpty(errorVal))
            ErrorMessage = Uri.UnescapeDataString(errorVal!);
        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(s => !string.IsNullOrEmpty(s.DisplayName) && s.Name != "SmartScheme")
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= "/";

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null || !user.IsActive)
        {
            ErrorMessage = ErrorMessages.InvalidCredentials;
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var decoded = System.Net.WebUtility.HtmlDecode(ReturnUrl);
            if (Url.IsLocalUrl(decoded))
                return LocalRedirect(decoded);
            return RedirectToFrontendWithTicket(user, decoded);
        }

        if (result.IsLockedOut)
        {
            ErrorMessage = ErrorMessages.AccountLocked;
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            ErrorMessage = ErrorMessages.TwoFactorRequired;
            return Page();
        }

        ErrorMessage = ErrorMessages.InvalidCredentials;
        return Page();
    }

    public IActionResult OnPostExternalLogin(string provider, string? returnUrl)
    {
        returnUrl ??= "/";
        var redirectUrl = Url.Page("./Login", pageHandler: "ExternalCallback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetExternalCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= "/";
        _logger.LogInformation("ExternalCallback: returnUrl={ReturnUrl}, remoteError={RemoteError}", returnUrl, remoteError);

        if (remoteError != null)
        {
            _logger.LogWarning("External callback remote error: {RemoteError}", remoteError);
            ErrorMessage = $"Erreur du fournisseur externe : {remoteError}";
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }

        try
        {
            var (user, roles) = await _authService.HandleExternalLoginAsync("external");
            return SafeOrTicketRedirect(user, returnUrl, roles.ToList());
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("External login failed: {Message}", ex.Message);
            ErrorMessage = ex.Message;
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }
    }

    private IActionResult SafeLocalRedirect(string url)
    {
        return Url.IsLocalUrl(url) ? LocalRedirect(url) : RedirectToPage("/Index");
    }

    private IActionResult SafeOrTicketRedirect(ApplicationUser user, string returnUrl, List<string> roles)
    {
        if (Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToFrontendWithTicket(user, returnUrl);
    }

    private IActionResult RedirectToFrontendWithTicket(ApplicationUser user, string returnUrl)
    {
        var ticket = Guid.NewGuid().ToString("N");
        var cacheEntry = new ExternalLoginTicket
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            TenantId = user.TenantId,
            IsActive = user.IsActive
        };
        _cache.Set($"external_login_{ticket}", cacheEntry, TimeSpan.FromMinutes(5));

        var separator = returnUrl.Contains('?') ? '&' : '?';
        return Redirect($"{returnUrl}{separator}ticket={ticket}");
    }
}
