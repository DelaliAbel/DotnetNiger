// Service applicatif Identity: UserService
using System.ComponentModel.DataAnnotations;
using DotnetNiger.Identity.Application.DTOs.Requests;
using DotnetNiger.Identity.Application.DTOs.Responses;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Mappers;
using DotnetNiger.Identity.Application.Services.Interfaces;
using DotnetNiger.Identity.Application.Validators;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Caching;
using DotnetNiger.Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Identity.Application.Services;

// Service de gestion du profil et du compte.
public class UserService : IUserService
{
    // Lecture du profil utilisateur.
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DotnetNigerIdentityDbContext _dbContext;
    private readonly ICacheService _cache;
    private static readonly TimeSpan ProfileCacheTtl = TimeSpan.FromMinutes(10);

    public UserService(
        UserManager<ApplicationUser> userManager,
        DotnetNigerIdentityDbContext dbContext,
        ICacheService cache)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _cache = cache;
    }

    private static string ProfileCacheKey(Guid userId) => $"user:profile:{userId}";

    public async Task<UserResponse> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var cacheKey = ProfileCacheKey(userId);
        var cached = await _cache.GetAsync<UserResponse>(cacheKey);
        if (cached is not null)
        {
            return cached;
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var dto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);
        await _cache.SetAsync(cacheKey, dto, ProfileCacheTtl);
        return dto;
    }

    public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        UpdateProfileRequestValidator.ValidateAndThrow(request);
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.Bio is not null) user.Bio = request.Bio;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;
        if (request.Country is not null) user.Country = request.Country;
        if (request.City is not null) user.City = request.City;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }

        var dto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);
        await _cache.RemoveAsync(ProfileCacheKey(userId));
        return dto;
    }

    public async Task<UserResponse> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
        {
            throw new IdentityException("AvatarUrl is required.", 400);
        }

        if (!new UrlAttribute().IsValid(avatarUrl))
        {
            throw new IdentityException("AvatarUrl is invalid.", 400);
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        user.AvatarUrl = avatarUrl;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }

        var dto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);
        await _cache.RemoveAsync(ProfileCacheKey(userId));
        return dto;
    }

    public async Task<UserResponse> ClearAvatarAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        user.AvatarUrl = string.Empty;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }

        var dto = await UserMapper.ToUserDtoAsync(user, _userManager, _dbContext);
        await _cache.RemoveAsync(ProfileCacheKey(userId));
        return dto;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }
    }

    public async Task ChangeEmailAsync(Guid userId, ChangeEmailRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            throw new UserNotFoundException();
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid)
        {
            throw new InvalidCredentialsException();
        }

        var newEmail = request.NewEmail?.Trim();
        if (string.IsNullOrWhiteSpace(newEmail))
        {
            throw new IdentityException("Email is required.", 400);
        }

        var existing = await _userManager.FindByEmailAsync(newEmail);
        if (existing != null && existing.Id != user.Id)
        {
            throw new IdentityException("Email already in use.", 409);
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new IdentityException(message, 400);
        }
    }
}
