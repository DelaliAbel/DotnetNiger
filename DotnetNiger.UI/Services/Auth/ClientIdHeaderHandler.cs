using System.Net;
using System.Net.Http.Headers;
using DotnetNiger.UI.Services.Contracts;
using Microsoft.AspNetCore.Components;

namespace DotnetNiger.UI.Services.Auth;

public class ClientIdHeaderHandler : DelegatingHandler
{
    private readonly ClientIdentifierProvider _clientIdentifierProvider;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClientIdHeaderHandler> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public ClientIdHeaderHandler(
        ClientIdentifierProvider clientIdentifierProvider,
        CustomAuthStateProvider authStateProvider,
        IServiceProvider serviceProvider,
        ILogger<ClientIdHeaderHandler> logger,
        NavigationManager navigationManager)
    {
        _clientIdentifierProvider = clientIdentifierProvider;
        _authStateProvider = authStateProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("ClientId"))
        {
            var clientId = await _clientIdentifierProvider.GetClientIdAsync();
            request.Headers.TryAddWithoutValidation("ClientId", clientId);
            _logger.LogDebug("Header ClientId injecté: {ClientId} sur {Method} {Uri}", clientId, request.Method, request.RequestUri);
        }

        if (request.Headers.Authorization is null)
        {
            var token = await _authStateProvider.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized
            && request.Headers.Authorization != null
            && request.RequestUri != null
            && !request.RequestUri.AbsolutePath.Contains("connect/token"))
        {
            // Acquire lock to prevent concurrent refresh attempts
            if (!await _refreshLock.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken))
            {
                _logger.LogWarning("Impossible d'acquérir le verrou de rafraîchissement, abandon");
                return response;
            }

            try
            {
                _logger.LogInformation("Tentative de rafraîchissement du token après 401 sur {Method} {Uri}", request.Method, request.RequestUri);

                var authService = _serviceProvider.GetRequiredService<IAuthService>();
                var refreshed = await authService.RefreshTokenAsync();

                if (refreshed?.Token?.AccessToken is not null)
                {
                    var clone = await CloneRequestAsync(request, cancellationToken);
                    clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.Token.AccessToken);
                    response = await base.SendAsync(clone, cancellationToken);
                    _logger.LogInformation("Requête réessayée avec succès après rafraîchissement du token");
                }
                else
                {
                    // Refresh failed - tokens already cleared by AuthService
                    // Check if we got a new token from storage (concurrent refresh succeeded)
                    var newToken = await _authStateProvider.GetAccessTokenAsync();
                    if (!string.IsNullOrWhiteSpace(newToken))
                    {
                        var clone = await CloneRequestAsync(request, cancellationToken);
                        clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        response = await base.SendAsync(clone, cancellationToken);
                        _logger.LogInformation("Requête réessayée avec le token stocké après rafraîchissement concurrent");
                    }
                    else
                    {
                        // No valid token - redirect to login
                        _logger.LogWarning("Rafraîchissement échoué, redirection vers la page de connexion");
                        RedirectToLogin();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Échec du rafraîchissement du token: {Message}", ex.Message);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        return response;
    }

    private void RedirectToLogin()
    {
        try
        {
            var currentUrl = _navigationManager.Uri;
            var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(currentUrl)}";
            _navigationManager.NavigateTo(loginUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la redirection vers la page de connexion");
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync(ct);
            clone.Content = new ByteArrayContent(contentBytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        clone.Version = request.Version;

        return clone;
    }
}
