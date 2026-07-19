// DTO response Identity: AuthDto
namespace DotnetNiger.Client.Models.Responses;

public class AuthDto
{
    public UserDto User { get; set; } = null!;
    public TokenDto Token { get; set; } = null!;
}
