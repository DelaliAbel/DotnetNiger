using DotnetNiger.Domain.Email;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Domain.Interfaces;
using DotnetNiger.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Services;

public partial class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly DotnetNigerDbContext _db;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly SmtpOptions _smtp;
    private readonly IPermissionService _permissionService;
    private readonly AccountService _accountService;
    private readonly IMemoryCache _cache;

    public AuthService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        DotnetNigerDbContext db,
        IEmailSender<ApplicationUser> emailSender,
        IOptions<SmtpOptions> smtp,
        IPermissionService permissionService,
        AccountService accountService,
        IMemoryCache cache)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailSender = emailSender;
        _smtp = smtp.Value;
        _permissionService = permissionService;
        _accountService = accountService;
        _cache = cache;
    }
}
