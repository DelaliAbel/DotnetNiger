// Services/IUserStateService.cs
using DotnetNiger.UI.Models.Responses;

namespace DotnetNiger.UI.Services.Contracts;

public interface IUserStateService
{
    event Action? OnChange;
    
    // Propriétés
    UserDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string UserName { get; }
    bool IsAdmin { get; }
    bool IsCollaborator { get; }
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