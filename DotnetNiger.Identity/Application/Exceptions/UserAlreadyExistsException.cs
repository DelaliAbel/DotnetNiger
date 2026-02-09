namespace DotnetNiger.Identity.Application.Exceptions;

public class UserAlreadyExistsException : IdentityException
{
	public UserAlreadyExistsException(string message)
		: base(message, 409)
	{
	}
}
