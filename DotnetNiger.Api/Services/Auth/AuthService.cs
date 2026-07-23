using DotnetNiger.Api.Data.Email;
using DotnetNiger.Api.Entities;
using DotnetNiger.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Api.Services.Auth;

/// <summary>Service principal d'authentification gérant la connexion et les sessions utilisateur.</summary>
public partial class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly DotnetNigerDbContext _db;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IPermissionService _permissionService;
    private readonly AccountService _accountService;

    public AuthService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        DotnetNigerDbContext db,
        IEmailSender<ApplicationUser> emailSender,
        IOptions<SmtpOptions> smtp,
        IPermissionService permissionService,
        AccountService accountService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _permissionService = permissionService;
        _accountService = accountService;
    }
}
