namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>DTO pour initier un login externe (Google, GitHub, Microsoft).</summary>
public class ExternalLoginRequest
{
    /// <summary>Nom du provider externe (Google, GitHub, Microsoft).</summary>
    public string Provider { get; set; } = string.Empty;
}
