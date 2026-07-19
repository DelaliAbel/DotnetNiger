using DotnetNiger.Client.Helpers;
using DotnetNiger.Client.Models.Responses;
using DotnetNiger.Client.Services.Contracts;

namespace DotnetNiger.Client.Services.Mock;

public class UserStateService : IUserStateService
{
    public event Action? OnChange;
    
    private UserDto? _currentUser;
    private readonly ILocalStorageService _localStorage;
    private readonly IUserService _userService;
    
    public UserStateService(ILocalStorageService localStorage, IUserService userService)
    {
        _localStorage = localStorage;
        _userService = userService;
    }
    
    // Propriétés
    public UserDto? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null && _currentUser.IsActive;
    public Guid UserId => _currentUser?.Id ?? Guid.Empty;
    public string UserName => _currentUser?.FullName ?? string.Empty;
    public bool IsAdmin => _currentUser?.Roles.Any(r => RoleConstants.IsAdminRole(r)) ?? false;
    public string? UserRole => _currentUser?.Roles.FirstOrDefault();
    public List<string> Roles => _currentUser?.Roles ?? [];

    public bool HasRole(string role) =>
        _currentUser?.Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)) ?? false;
    
    // Méthodes
    public async Task LoadUserFromStorageAsync()
    {
        _currentUser = await _localStorage.GetItemAsync<UserDto>("dn_wasm_runtime_registry_member");
        OnChange?.Invoke();
    }
    
    public async Task SetUserAsync(UserDto user)
    {
        _currentUser = user;
        await _localStorage.SetItemAsync("dn_wasm_runtime_registry_member", user);
        OnChange?.Invoke();
    }
    
    public async Task UpdateUserAsync(UserDto updatedUser)
    {
        if (_currentUser != null && _currentUser.Id == updatedUser.Id)
        {
            _currentUser = updatedUser;
            await _localStorage.SetItemAsync("dn_wasm_runtime_registry_member", updatedUser);
            OnChange?.Invoke();
        }
    }
    
    public async Task ClearUserAsync()
    {
        _currentUser = null;
        await _localStorage.RemoveItemAsync("dn_wasm_runtime_registry_member");
        OnChange?.Invoke();
    }
    
    public async Task RefreshUserAsync()
    {
        if (_currentUser != null)
        {
            var freshUser = await _userService.GetUserByIdAsync(_currentUser.Id);
            if (freshUser != null)
            {
                _currentUser = freshUser;
                await _localStorage.SetItemAsync("dn_wasm_runtime_registry_member", freshUser);
                OnChange?.Invoke();
            }
        }
    }
}