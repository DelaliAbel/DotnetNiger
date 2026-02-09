namespace DotnetNiger.Identity.Application.Exceptions;

public class IdentityException : Exception
{
	public IdentityException(string message, int statusCode)
		: base(message)
	{
		StatusCode = statusCode;
	}

	public int StatusCode { get; }
}
