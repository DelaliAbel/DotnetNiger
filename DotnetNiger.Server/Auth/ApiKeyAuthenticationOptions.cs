using Microsoft.AspNetCore.Authentication;

namespace DotnetNiger.Infrastructure.Auth;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";
}
