namespace DotnetNiger.Identity.Application.Exceptions;

public class InvalidCredentialsException : IdentityException
{
	public InvalidCredentialsException()
		: base("Invalid credentials.", 401)
	{
	}
}
