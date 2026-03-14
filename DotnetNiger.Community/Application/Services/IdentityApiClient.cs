using DotnetNiger.Community.Application.Services.Interfaces;

namespace DotnetNiger.Community.Application.Services;

public class IdentityApiClient : IIdentityApiClient
{
	private readonly HttpClient _httpClient;

	public IdentityApiClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public string BaseUrl => _httpClient.BaseAddress?.ToString() ?? string.Empty;

	public async Task<bool> IsReachableAsync(CancellationToken cancellationToken = default)
	{
		using var response = await _httpClient.GetAsync("api/v1/health", cancellationToken);
		return response.IsSuccessStatusCode;
	}
}
