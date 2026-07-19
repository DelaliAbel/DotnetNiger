// Services/IUserStateService.cs
using DotnetNiger.Client.Models.Responses;

namespace DotnetNiger.Client.Services.Contracts;

public interface IUserStateService
{
    event Action? OnChange;
    
    // Propriétés
    UserDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string UserName { get; }
    bool IsAdmin { get; }
    string? UserRole { get; }
    List<string> Roles { get; }
    
    // Méthodes
    bool HasRole(string role);
    Task LoadUserFromStorageAsync();
    Task SetUserAsync(UserDto user);
    Task UpdateUserAsync(UserDto updatedUser);
    Task ClearUserAsync();
    Task RefreshUserAsync();
}