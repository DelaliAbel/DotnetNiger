namespace DotnetNiger.UI.Models.Requests;

public class UnsubscribeRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Token { get; set; }
}
