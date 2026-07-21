namespace DotnetNiger.Api.DTOs.Requests;

public class ConfirmEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
