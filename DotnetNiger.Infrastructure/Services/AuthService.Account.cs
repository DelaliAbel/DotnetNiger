using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.DTOs.Responses;
using DotnetNiger.Domain.Entities;
using DotnetNiger.Domain.Models;
using DotnetNiger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Services;

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

public partial class AuthService
{
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

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var (user, roles) = await ValidateCredentialsAsync(request.Email, request.Password);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        return new TokenResponse
        {
            AccessToken = "",
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _accountService.RegisterAsync(request.Email, request.Password, request.FirstName, request.LastName);
        var roles = await _userManager.GetRolesAsync(user);
        return new TokenResponse
        {
            AccessToken = "",
            ExpiresIn = 3600,
            UserId = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };
    }

    public Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TokenResponse
        {
            AccessToken = "",
            ExpiresIn = 3600
        });
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        await _accountService.ConfirmEmailAsync(request.Email, request.Code);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _accountService.ForgotPasswordAsync(request.Email);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _accountService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
    }
}
