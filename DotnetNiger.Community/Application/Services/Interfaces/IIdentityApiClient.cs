namespace DotnetNiger.Community.Application.Services.Interfaces;

public interface IIdentityApiClient
{
	Task<bool> IsReachableAsync(CancellationToken cancellationToken = default);
	string BaseUrl { get; }
}
