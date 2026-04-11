using System.Net.Http.Headers;

namespace DotnetNiger.UI.Services.Auth;

public class ClientIdHeaderHandler : DelegatingHandler
{
    private readonly ClientIdentifierProvider _clientIdentifierProvider;
    private readonly CustomAuthStateProvider _authStateProvider;
    private readonly ILogger<ClientIdHeaderHandler> _logger;

    public ClientIdHeaderHandler(
        ClientIdentifierProvider clientIdentifierProvider,
        CustomAuthStateProvider authStateProvider,
        ILogger<ClientIdHeaderHandler> logger)
    {
        _clientIdentifierProvider = clientIdentifierProvider;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("ClientId"))
        {
            var clientId = await _clientIdentifierProvider.GetClientIdAsync();
            request.Headers.TryAddWithoutValidation("ClientId", clientId);
            _logger.LogDebug("Header ClientId injecte: {ClientId} sur {Method} {Uri}", clientId, request.Method, request.RequestUri);
        }

        if (request.Headers.Authorization is null)
        {
            var token = await _authStateProvider.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
