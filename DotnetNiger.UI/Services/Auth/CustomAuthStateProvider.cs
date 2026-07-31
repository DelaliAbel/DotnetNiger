using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using DotnetNiger.UI.Models.Responses;
using DotnetNiger.UI.Services.Contracts;
using System.Threading;

namespace DotnetNiger.UI.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly IServiceProvider _serviceProvider;
    private static AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private static readonly SemaphoreSlim _refreshLock = new(1, 1);
    private static Task<AuthDto?>? _pendingRefresh;

    private const string AccessTokenKey = "dn_wasm_runtime_registry_key";
    private const string RefreshTokenKey = "dn_wasm_runtime_registry_renew";

    private string? _accessToken;

    public CustomAuthStateProvider(IJSRuntime js, IServiceProvider serviceProvider)
    {
        _js = js;
        _serviceProvider = serviceProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessTokenAsync();
        var refreshToken = await GetRefreshTokenAsync();

        if (!string.IsNullOrWhiteSpace(token) && string.IsNullOrWhiteSpace(refreshToken))
        {
            await ClearTokensAsync();
            return Anonymous;
        }

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        var claims = JwtParser.ParseClaimsFromJwt(token);
        var expClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (expClaim != null && long.TryParse(expClaim, out var expUnix))
        {
            var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix);
            if (expDate <= DateTimeOffset.UtcNow)
            {
                var authService = _serviceProvider.GetRequiredService<IAuthService>();
                AuthDto? refreshed;

                await _refreshLock.WaitAsync();
                try
                {
                    if (_pendingRefresh is null || _pendingRefresh.IsCompleted)
                        _pendingRefresh = authService.RefreshTokenAsync();
                }
                finally
                {
                    _refreshLock.Release();
                }

                try
                {
                    refreshed = await _pendingRefresh;
                }
                catch
                {
                    await ClearTokensAsync();
                    return Anonymous;
                }

                await _refreshLock.WaitAsync();
                try
                {
                    if (_pendingRefresh?.IsCompleted == true)
                        _pendingRefresh = null;
                }
                finally
                {
                    _refreshLock.Release();
                }

                if (refreshed?.Token?.AccessToken is not null)
                {
                    token = refreshed.Token.AccessToken;
                    claims = JwtParser.ParseClaimsFromJwt(token);
                }
                else
                {
                    return Anonymous;
                }
            }
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken))
            return _accessToken;

        try
        {
            _accessToken = await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
            return _accessToken;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        _accessToken = accessToken;

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, accessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthState] SaveTokens: échec écriture localStorage — {ex.Message}");
        }
        NotifyAuthenticationStateChanged(Task.FromResult(CreateAuthenticatedState(accessToken)));
    }

    private static AuthenticationState CreateAuthenticatedState(string token)
    {
        var claims = JwtParser.ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task ClearTokensAsync(bool clearAllStorage = true, CancellationToken cancellationToken = default)
    {
        _accessToken = null;

        try
        {
            // Clear auth tokens
            await _js.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
            await _js.InvokeVoidAsync("localStorage.removeItem", "dn_wasm_runtime_registry_member");
            
            // Optional: Clear all storage for maximum security
            if (clearAllStorage)
            {
                await _js.InvokeVoidAsync("localStorage.clear");
                await _js.InvokeVoidAsync("sessionStorage.clear");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthState] ClearTokens: échec suppression localStorage — {ex.Message}");
        }
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

}
