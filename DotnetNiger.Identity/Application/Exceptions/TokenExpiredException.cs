namespace DotnetNiger.Identity.Application.Exceptions;

public class TokenExpiredException : IdentityException
{
	public TokenExpiredException()
		: base("Refresh token expired.", 401)
	{
	}
}
