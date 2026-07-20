using OpenIddict.Abstractions;
using DotnetNiger.Domain.DTOs.Requests;
using DotnetNiger.Domain.Entities;

namespace DotnetNiger.Infrastructure.Services.General;

public partial class OpenIddictClientManager
{
    private static OpenIddictApplicationDescriptor BuildClientDescriptor(CreateOAuthClientRequest request, string clientId, string clientSecret)
    {
        var grantTypes = OAuthClientService.ParseJsonArrayOrDefault(request.AllowedGrantTypes,
            ["authorization_code", "password", "refresh_token", "client_credentials"]);

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId, ClientSecret = clientSecret,
            DisplayName = request.ClientName,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
        };
        AddPermissions(descriptor, grantTypes);

        foreach (var uri in OAuthClientService.ParseJsonArray(request.RedirectUris))
            descriptor.RedirectUris.Add(new Uri(uri));
        foreach (var uri in OAuthClientService.ParseJsonArray(request.PostLogoutRedirectUris))
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri));

        return descriptor;
    }

    private static bool UpdateClientUris(OAuthClient oAuthClient, OpenIddictApplicationDescriptor descriptor, UpdateOAuthClientRequest request)
    {
        var updated = false;
        if (request.RedirectUris != null)
        {
            oAuthClient.RedirectUris = request.RedirectUris;
            descriptor.RedirectUris.Clear();
            foreach (var uri in OAuthClientService.ParseJsonArray(request.RedirectUris))
                descriptor.RedirectUris.Add(new Uri(uri));
            updated = true;
        }

        if (request.PostLogoutRedirectUris != null)
        {
            oAuthClient.PostLogoutRedirectUris = request.PostLogoutRedirectUris;
            descriptor.PostLogoutRedirectUris.Clear();
            foreach (var uri in OAuthClientService.ParseJsonArray(request.PostLogoutRedirectUris))
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
            updated = true;
        }

        return updated;
    }

    private static bool UpdateClientPermissions(OpenIddictApplicationDescriptor descriptor, UpdateOAuthClientRequest request, OAuthClient oAuthClient)
    {
        if (request.AllowedGrantTypes == null) return false;

        oAuthClient.AllowedGrantTypes = request.AllowedGrantTypes;
        var grants = OAuthClientService.ParseJsonArray(request.AllowedGrantTypes);
        descriptor.Permissions.Clear();
        AddPermissions(descriptor, grants);
        return true;
    }

    private static void AddPermissions(OpenIddictApplicationDescriptor descriptor, List<string> grantTypes)
    {
        descriptor.Permissions.Add("ept:token");
        descriptor.Permissions.Add("ept:authorization");
        descriptor.Permissions.Add("ept:logout");
        descriptor.Permissions.Add("ept:userinfo");
        foreach (var grant in grantTypes)
        {
            descriptor.Permissions.Add(grant switch
            {
                "authorization_code" => "gt:authorization_code",
                "password" => "gt:password",
                "refresh_token" => "gt:refresh_token",
                "client_credentials" => "gt:client_credentials",
                _ => throw new InvalidOperationException($"Grant type non supporté : {grant}")
            });
        }
        descriptor.Permissions.Add("scp:openid");
        descriptor.Permissions.Add("scp:email");
        descriptor.Permissions.Add("scp:profile");
        descriptor.Permissions.Add("scp:roles");
        descriptor.Permissions.Add("scp:offline_access");
        descriptor.Permissions.Add("scp:api");
    }
}
