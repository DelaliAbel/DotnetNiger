using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using DotnetNiger.Domain.Constants;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Infrastructure.Services;

namespace DotnetNiger.Server.Pages.Account;

[EnableRateLimiting("Auth")]
public partial class LoginModel : PageModel
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

    public string? EmailConfirmMessage { get; set; }

    public IList<AuthenticationScheme> ExternalProviders { get; set; } = new List<AuthenticationScheme>();

    public async Task OnGetAsync()
    {
        ReturnUrl ??= "/";

        if (Request.Query.TryGetValue("error", out var errorVal) && !string.IsNullOrEmpty(errorVal))
            ErrorMessage = Uri.UnescapeDataString(errorVal!);

        if (Request.Query.TryGetValue("emailConfirmed", out var confirmed) && confirmed == "true")
            EmailConfirmMessage = "Votre email a ete confirme avec succes. Vous pouvez maintenant vous connecter.";

        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(s => !string.IsNullOrEmpty(s.DisplayName) && s.Name != "SmartScheme")
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= "/";

        try
        {
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName) && s.Name != "SmartScheme")
                .ToList();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", Email);
            ErrorMessage = "Une erreur est survenue lors de la connexion. Veuillez reessayer plus tard.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostResendConfirmationAsync(string confirmEmail, string? returnUrl)
    {
        ReturnUrl = returnUrl ?? "/";
        ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(s => !string.IsNullOrEmpty(s.DisplayName) && s.Name != "SmartScheme")
            .ToList();

        if (string.IsNullOrWhiteSpace(confirmEmail))
        {
            ErrorMessage = "Veuillez saisir votre adresse email.";
            return Page();
        }

        var accountService = HttpContext.RequestServices.GetRequiredService<AccountService>();
        try
        {
            await accountService.ResendConfirmationCodeAsync(confirmEmail);
            EmailConfirmMessage = $"Un nouveau code de confirmation a ete envoye a {confirmEmail}.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
}
