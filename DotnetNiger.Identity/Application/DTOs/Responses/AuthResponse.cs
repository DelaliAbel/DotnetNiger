namespace DotnetNiger.Identity.Application.DTOs.Responses;

public class AuthResponse
{
    public UserResponse User { get; set; } = null!;
    public TokenResponse Token { get; set; } = null!;
}
