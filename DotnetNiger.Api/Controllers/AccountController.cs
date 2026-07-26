using System.Security.Claims;
using DotnetNiger.Api.Constants;
using DotnetNiger.Api.DTOs.Requests;
using DotnetNiger.Api.DTOs.Responses;
using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DotnetNiger.Api.Entities;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Api.Controllers;

/// <summary>
/// Controller principal d'authentification.
/// Garde les mêmes routes que le frontend attend (api/auth/*).
/// Utilise TokenService natif Microsoft au lieu d'OpenIddict.
/// </summary>
[ApiController]
[Route("api/auth")]
[EnableRateLimiting("Auth")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly TokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly SmtpOptions _smtp;

    public AccountController(
        AccountService accountService,
        TokenService tokenService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<SmtpOptions> smtp)
    {
        _accountService = accountService;
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
        _smtp = smtp.Value;
    }

    // ============================================================
    // INSCRIPTION
    // ============================================================

    /// <summary>Crée un compte et envoie un email de confirmation.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Données invalides", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        try
        {
            var user = await _accountService.RegisterAsync(
                request.Email, request.Password,
                request.FirstName, request.LastName, request.PhoneNumber);

            return Ok(new
            {
                message = "Compte créé. Un code de confirmation vous a été envoyé par email.",
                userId = user.Id,
                email = user.Email
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ============================================================
    // CONNEXION (login natif → génère JWT)
    // ============================================================

    /// <summary>
    /// Connecte l'utilisateur et retourne un JWT access token + refresh token.
    /// Le JWT contient les rôles natifs Microsoft + les permissions custom.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.IsActive)
                return Unauthorized(new { error = "Email ou mot de passe incorrect" });

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { error = "Email non confirmé. Vérifiez votre boîte de réception." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (result.IsLockedOut)
                return Unauthorized(new { error = "Compte temporairement verrouillé (trop de tentatives)" });
            if (!result.Succeeded)
                return Unauthorized(new { error = "Email ou mot de passe incorrect" });

            var (accessToken, refreshToken, expiresIn) =
                await _tokenService.GenerateTokenPairAsync(user, request.RememberMe);

            return Ok(new
            {
                accessToken,
                refreshToken,
                expiresIn,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = $"{user.FirstName} {user.LastName}".Trim(),
                    avatarUrl = user.AvatarUrl,
                    roles = await _userManager.GetRolesAsync(user)
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    // ============================================================
    // RAFRAÎCHISSEMENT DU TOKEN
    // ============================================================

    /// <summary>
    /// Valide le refresh token et retourne un nouveau couple de tokens.
    /// La rotation est obligatoire : l'ancien token est révoqué.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<object>> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { error = "Refresh token requis" });

        var result = await _tokenService.RotateRefreshTokenAsync(request.RefreshToken);
        if (result is null)
            return Unauthorized(new { error = "Refresh token invalide ou expiré" });

        var (accessToken, refreshToken, expiresIn) = result.Value;
        return Ok(new { accessToken, refreshToken, expiresIn });
    }

    // ============================================================
    // DÉCONNEXION
    // ============================================================

    /// <summary>Révoque le refresh token de cet appareil.</summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        if (request?.RefreshToken is not null)
            await _tokenService.RevokeTokenAsync(request.RefreshToken);

        await _signInManager.SignOutAsync();
        return Ok(new { message = "Déconnecté avec succès" });
    }

    // ============================================================
    // LOGIN EXTERNE (Google / GitHub / Microsoft) — POPUP
    // ============================================================

    /// <summary>
    /// Initie le login externe via popup. Le navigateur redirige vers le provider
    /// (Google/GitHub/Microsoft). Après authentification, le provider redirige
    /// vers /signin-{provider} sur l'API, puis l'API sert une page HTML qui
    /// envoie les tokens JWT à la fenêtre parent via postMessage et se ferme.
    /// </summary>
    [HttpGet("external-login")]
    public ActionResult ExternalLogin([FromQuery] string provider)
    {
        var callbackUrl = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/api/auth/external-callback";

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { { "LoginProvider", provider } }
        };

        return Challenge(properties, provider);
    }

    /// <summary>
    /// Callback après login externe (appelé par le provider OAuth).
    /// Lit les infos du provider, crée l'utilisateur si nécessaire,
    /// génère les JWT, et sert une page HTML qui postMessage les tokens
    /// à la fenêtre parent puis se ferme automatiquement.
    /// </summary>
    [HttpGet("external-callback")]
    public async Task ExternalCallback()
    {
        Response.ContentType = "text/html";

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            await WriteErrorAndClose(Response, "Connexion externe échouée.");
            return;
        }

        var user = await ProcessExternalLoginAsync(info);
        if (user is null)
        {
            await WriteErrorAndClose(Response, "Échec de la connexion externe.");
            return;
        }

        var (accessToken, refreshToken, expiresIn) =
            await _tokenService.GenerateTokenPairAsync(user);

        var frontendOrigin = _smtp.FrontendBaseUrl.TrimEnd('/');
        var html = "<!DOCTYPE html><html><head><title>Connexion en cours...</title></head><body>"
            + "<script>"
            + "window.opener.postMessage({"
            + $"  type: 'external-login-success',"
            + $"  accessToken: '{accessToken}',"
            + $"  refreshToken: '{refreshToken}',"
            + $"  expiresIn: {expiresIn}"
            + $"}}, '{frontendOrigin}');"
            + "window.close();"
            + "</script>"
            + "<p>Connexion réussie. Cette fenêtre se ferme automatiquement...</p>"
            + "</body></html>";

        await Response.WriteAsync(html);
    }

    /// <summary>Sert une page HTML d'erreur et ferme la popup.</summary>
    private async Task WriteErrorAndClose(HttpResponse response, string message)
    {
        response.ContentType = "text/html";
        var frontendOrigin = _smtp.FrontendBaseUrl.TrimEnd('/');
        var html = "<!DOCTYPE html><html><head><title>Erreur</title></head><body>"
            + "<script>"
            + "window.opener.postMessage({ type: 'external-login-error', error: '" + message + "' }, '" + frontendOrigin + "');"
            + "window.close();"
            + "</script>"
            + "<p>Erreur : " + message + "</p>"
            + "</body></html>";
        await response.WriteAsync(html);
    }

    // ============================================================
    // CONFIRMATION EMAIL
    // ============================================================

    /// <summary>Confirme l'email avec le code envoyé par email.</summary>
    [HttpPost("confirm-email")]
    public async Task<ActionResult<object>> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _accountService.ConfirmEmailAsync(request.Email, request.Code);
        return Ok(new { message = "Email confirmé avec succès. Vous pouvez maintenant vous connecter." });
    }

    /// <summary>Confirmation email via lien cliqué dans le mail (GET).</summary>
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailGet(
        [FromQuery] string email, [FromQuery] string code)
    {
        await _accountService.ConfirmEmailAsync(email, code);
        var redirectUrl = $"{_smtp.FrontendBaseUrl.TrimEnd('/')}/login?emailConfirmed=true";
        return Redirect(redirectUrl);
    }

    // ============================================================
    // RENVOI DU CODE DE CONFIRMATION
    // ============================================================

    /// <summary>Regénère et renvoie le code de confirmation email.</summary>
    [HttpPost("resend-code")]
    public async Task<ActionResult<object>> ResendCode([FromBody] ForgotPasswordRequest request)
    {
        await _accountService.ResendConfirmationCodeAsync(request.Email);
        return Ok(new { message = "Un nouveau code de confirmation vous a été envoyé." });
    }

    // ============================================================
    // ALIAS : verify-email + request-email-verification
    // ============================================================

    /// <summary>Vérifie l'email avec le code (alias de confirm-email).</summary>
    [HttpPost("verify-email")]
    public async Task<ActionResult<object>> VerifyEmail([FromBody] ConfirmEmailRequest request)
    {
        try
        {
            await _accountService.ConfirmEmailAsync(request.Email, request.Code);
            return Ok(new { message = "Email confirmé avec succès." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Demande un nouveau code de vérification email (alias de resend-code).</summary>
    [HttpPost("request-email-verification")]
    public async Task<ActionResult<object>> RequestEmailVerification([FromBody] ForgotPasswordRequest request)
    {
        await _accountService.ResendConfirmationCodeAsync(request.Email);
        return Ok(new { message = "Un nouveau code de vérification vous a été envoyé." });
    }

    // ============================================================
    // INFO UTILISATEUR (depuis le JWT)
    // ============================================================

    /// <summary>Retourne les informations de l'utilisateur connecté depuis le JWT.</summary>
    [HttpGet("userinfo")]
    [Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsForRolesAsync(roles);

        return Ok(new UserInfoResponse(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AvatarUrl, user.IsActive, roles.ToList(), permissions));
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim is null) return null;
        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    private static Task<IList<string>> GetPermissionsForRolesAsync(IList<string> roles)
    {
        var permissions = new List<string>();
        foreach (var role in roles)
        {
            var rolePerms = role switch
            {
                RoleConstants.SuperAdmin => Permissions.All,
                RoleConstants.Admin => Permissions.AdminPermissions,
                RoleConstants.Collaborator => Permissions.CollaboratorPermissions,
                RoleConstants.User => Permissions.UserPermissions,
                _ => []
            };
            permissions.AddRange(rolePerms);
        }
        return Task.FromResult<IList<string>>(permissions.Distinct().ToList());
    }

    // ============================================================
    // MOT DE PASSE OUBLIÉ / RÉINITIALISATION
    // ============================================================

    /// <summary>Envoie un email de réinitialisation de mot de passe.</summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _accountService.ForgotPasswordAsync(request.Email);
        return Ok(new { message = "Si le compte existe, un email de réinitialisation a été envoyé." });
    }

    /// <summary>Réinitialise le mot de passe avec le token reçu par email.</summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var error = await _accountService.ResetPasswordAsync(
            request.Email, request.Token, request.NewPassword);

        if (error is not null)
            return BadRequest(new { message = error });

        return Ok(new { message = "Mot de passe réinitialisé avec succès." });
    }

    // ============================================================
    // UTILITAIRE : traitement du login externe
    // ============================================================

    /// <summary>
    /// Traite le login externe : lie le compte existant ou crée un nouveau compte.
    /// Assigne le rôle "User" par défaut aux nouveaux utilisateurs.
    /// </summary>
    private async Task<ApplicationUser?> ProcessExternalLoginAsync(ExternalLoginInfo info)
    {
        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded)
        {
            return await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }

        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return null;

        // Cherche un compte existant avec le même email
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            await _userManager.AddLoginAsync(existingUser, info);
            existingUser.EmailConfirmed = true;
            await _userManager.UpdateAsync(existingUser);
            return existingUser;
        }

        // Crée un nouveau compte
        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value,
            LastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
            return null;

        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "User");
        return newUser;
    }
}
