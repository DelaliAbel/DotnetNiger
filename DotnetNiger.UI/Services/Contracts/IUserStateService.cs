// Services/IUserStateService.cs
using DotnetNiger.UI.Models.Responses;
using System.Threading;

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
    Task LoadUserFromStorageAsync(CancellationToken cancellationToken = default);
    Task SetUserAsync(UserDto user, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(UserDto updatedUser, CancellationToken cancellationToken = default);
    Task ClearUserAsync(CancellationToken cancellationToken = default);
    Task RefreshUserAsync(CancellationToken cancellationToken = default);
}