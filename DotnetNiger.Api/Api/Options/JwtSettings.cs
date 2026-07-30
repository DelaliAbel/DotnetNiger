namespace DotnetNiger.Api.Options;

/// <summary>
/// Configuration JWT injectée depuis appsettings.json (section "Jwt").
/// Contient la clé de signature, l'émetteur, l'audience et les durées de vie.
/// </summary>
public class JwtSettings
{
    /// <summary>Nom de la section dans appsettings.json.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Clé secrète pour signer les tokens (min 32 caractères).</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Émetteur du token (ex: "DotnetNiger API").</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Audience du token (ex: "DotnetNiger WebUI").</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Durée de vie de l'access token en minutes.</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>Durée de vie du refresh token en jours.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
