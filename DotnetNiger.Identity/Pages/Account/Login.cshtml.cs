using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;

namespace DotnetNiger.Identity.Pages.Account;

[EnableRateLimiting("Auth")]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _db;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IdentityDbContext db,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _logger = logger;
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
            .Where(s => !string.IsNullOrEmpty(s.DisplayName))
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl ??= "/";

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null || !user.IsActive)
        {
            ErrorMessage = "Email ou mot de passe incorrect.";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var decoded = System.Net.WebUtility.HtmlDecode(ReturnUrl ?? "/");
            return SafeRedirect(decoded);
        }

        if (result.IsLockedOut)
        {
            ErrorMessage = "Compte temporairement verrouillé. Réessayez plus tard.";
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            ErrorMessage = "Authentification à deux facteurs requise (non configurée).";
            return Page();
        }

        ErrorMessage = "Email ou mot de passe incorrect.";
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

        var info = await _signInManager.GetExternalLoginInfoAsync();
        _logger.LogInformation("ExternalLoginInfo: {Info}", info != null ? "found" : "null");
        if (info == null)
        {
            _logger.LogWarning("ExternalLoginInfo is null - checking authentication properties");
            var authProps = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            _logger.LogWarning("External auth result: Succeeded={Succeeded}, Failure={Failure}, Ticket.Principal={Principal}",
                authProps.Succeeded, authProps.Failure?.Message, authProps.Ticket?.Principal?.Identity?.Name);
            ErrorMessage = "Erreur lors de la connexion externe.";
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var firstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
        var lastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
            return SafeRedirect(returnUrl);

        if (result.IsLockedOut)
        {
            ErrorMessage = "Compte temporairement verrouillé.";
            return Page();
        }

        if (string.IsNullOrEmpty(email))
        {
            ErrorMessage = "Impossible de récupérer votre email depuis le fournisseur externe.";
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            await _userManager.AddLoginAsync(existingUser, info);
            existingUser.EmailConfirmed = true;
            await _userManager.UpdateAsync(existingUser);
            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            return SafeRedirect(returnUrl);
        }

        var tenant = await _db.Tenants.FirstOrDefaultAsync();
        if (tenant == null)
        {
            ErrorMessage = "Aucun tenant configuré. Veuillez contacter l'administrateur.";
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            TenantId = tenant.Id,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            ErrorMessage = string.Join(" ", createResult.Errors.Select(e => e.Description));
            ExternalProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName)).ToList();
            return Page();
        }

        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "User");
        await _signInManager.SignInAsync(newUser, isPersistent: false);

        _logger.LogInformation("New user created via external login: {Email}", email);
        return SafeRedirect(returnUrl);
    }

    private IActionResult SafeRedirect(string url)
    {
        return Url.IsLocalUrl(url) ? LocalRedirect(url) : RedirectToPage("/Index");
    }
}
