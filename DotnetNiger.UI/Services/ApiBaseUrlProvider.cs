namespace DotnetNiger.UI.Services;

public class ApiBaseUrlProvider
{
    public string BaseUrl { get; }

    public ApiBaseUrlProvider(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
    }
}
