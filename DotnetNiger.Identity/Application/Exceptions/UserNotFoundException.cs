namespace DotnetNiger.Identity.Application.Exceptions;

public class UserNotFoundException : IdentityException
{
	public UserNotFoundException()
		: base("User not found.", 404)
	{
	}
}
