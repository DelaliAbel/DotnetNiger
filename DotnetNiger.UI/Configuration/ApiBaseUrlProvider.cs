namespace DotnetNiger.UI.Configuration;

public class ApiBaseUrlProvider
{
    public string BaseUrl { get; }

    public ApiBaseUrlProvider(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
    }
}
