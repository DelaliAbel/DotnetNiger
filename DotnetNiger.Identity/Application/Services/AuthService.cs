// Service applicatif Identity: AuthService
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Abstractions.Persistence;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Mappers;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Application.Validators;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data;
using DotnetNiger.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Identity.Application.Services;

// Service d'authentification et de gestion des tokens.
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly DotnetNigerIdentityDbContext _dbContext;
    private readonly IRefreshTokenPersistence _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly IEmailService _emailService;
    private readonly ILoginHistoryService _loginHistoryService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IEmailVerificationCodeService _emailVerificationCodeService;
    private readonly IAppSettingPersistence _appSettingRepository;
    private readonly ICommunityProvisioningClient _communityProvisioningClient;
    private readonly ILogger<AuthService> _logger;
    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        DotnetNigerIdentityDbContext dbContext,
        IRefreshTokenPersistence refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IOptions<JwtOptions> jwtOptions,
        IEmailService emailService,
        ILoginHistoryService loginHistoryService,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IEmailVerificationCodeService emailVerificationCodeService,
        IAppSettingPersistence appSettingRepository,
        ICommunityProvisioningClient communityProvisioningClient,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _jwtOptions = jwtOptions.Value;
        _emailService = emailService;
        _loginHistoryService = loginHistoryService;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _emailVerificationCodeService = emailVerificationCodeService;
        _appSettingRepository = appSettingRepository;
        _communityProvisioningClient = communityProvisioningClient;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        RegisterRequestValidator.ValidateAndThrow(request);
        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail != null)
        {
            throw new UserAlreadyExistsException("Email already in use.");
        }

        var existingByUsername = await _userManager.FindByNameAsync(request.Username);
        if (existingByUsername != null)
        {
            throw new UserAlreadyExistsException("Username already in use.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            Country = request.Country,
            City = request.City,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }

        // Configurable default role (must stay within allowed business roles).
        var defaultRole = _appSettingRepository.GetValue("Auth:DefaultRole")
            ?? _configuration["Auth:DefaultRole"]
            ?? "Member";

        var allowedRoles = new[] { "Member", "Admin", "SuperAdmin" };
        if (!allowedRoles.Contains(defaultRole, StringComparer.OrdinalIgnoreCase))
        {
            defaultRole = "Member";
        }

        if (await _roleManager.RoleExistsAsync(defaultRole))
        {
            await _userManager.AddToRoleAsync(user, defaultRole);
        }

        try
        {
            await _communityProvisioningClient.ProvisionPendingMemberAsync(user.Id, user.FullName, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Community provisioning failed for user {UserId}. Rolling back Identity user creation.", user.Id);
            await _userManager.DeleteAsync(user);
            throw new IdentityException("Registration failed during community provisioning. Please retry later.", 503);
        }

        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var verificationCode = await _emailVerificationCodeService.CreateCodeAsync(user.Email ?? string.Empty, confirmationToken, ct);
        await _emailService.SendAsync(
            user.Email ?? string.Empty,
            "Verify email",
            $"Your verification code is: {verificationCode}. It expires in 10 minutes.");

        var tokenDto = await CreateTokenAsync(user);
        var userDto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);

        return new AuthResponse
        {
            User = userDto,
            Token = tokenDto
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        LoginRequestValidator.ValidateAndThrow(request);
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            await _loginHistoryService.RecordAsync(user.Id, false, "User disabled.");
            throw new IdentityException("User is disabled.", 403);
        }

        if (!user.EmailConfirmed)
        {
            await _loginHistoryService.RecordAsync(user.Id, false, "Email not verified.");
            throw new IdentityException("Email is not verified.", 403);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _loginHistoryService.RecordAsync(user.Id, false, "Invalid credentials.");
            throw new InvalidCredentialsException();
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await _loginHistoryService.RecordAsync(user.Id, true, string.Empty);

        var tokenDto = await CreateTokenAsync(user);
        var userDto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);

        return new AuthResponse
        {
            User = userDto,
            Token = tokenDto
        };
    }

    public async Task<string?> RequestEmailVerificationAsync(RequestEmailVerificationRequest request, CancellationToken ct = default)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new IdentityException("Email is required.", 400);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        if (user.EmailConfirmed)
        {
            return null;
        }

        var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var verificationCode = await _emailVerificationCodeService.CreateCodeAsync(email, identityToken, ct);
        await _emailService.SendAsync(
            email,
            "Verify email",
            $"Your verification code is: {verificationCode}. It expires in 10 minutes.");

        return verificationCode;
    }

    public async Task<string?> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new IdentityException("Email is required.", 400);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendAsync(email, "Reset password", $"Your reset token: {token}");
        return token;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        ResetPasswordRequestValidator.ValidateAndThrow(request);
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new IdentityException("Email is required.", 400);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new IdentityException("Invalid reset request.", 400);
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new IdentityException("Email is required.", 400);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new IdentityException("Invalid verification request.", 400);
        }

        var identityToken = await _emailVerificationCodeService.ConsumeIdentityTokenAsync(email, request.Token, ct);
        if (string.IsNullOrWhiteSpace(identityToken))
        {
            throw new IdentityException("Invalid or expired verification code.", 400);
        }

        var result = await _userManager.ConfirmEmailAsync(user, identityToken);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }
    }

    private async Task<TokenResponse> CreateTokenAsync(ApplicationUser user)
    {
        var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
        var refreshTokenValue = _refreshTokenGenerator.GenerateToken();
        var hashedToken = _refreshTokenGenerator.HashToken(refreshTokenValue);

        var httpContext = _httpContextAccessor.HttpContext;
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        // On retourne le token brut au client ; seul le hash est stocke en base.
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = _jwtOptions.AccessTokenMinutes * 60,
            TokenType = "Bearer"
        };
    }

    public async Task AssignMemberRoleAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (!await _roleManager.RoleExistsAsync("Member"))
        {
            throw new IdentityException("Role 'Member' does not exist.", 500);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains("Member", StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, "Member");
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }
    }

}
