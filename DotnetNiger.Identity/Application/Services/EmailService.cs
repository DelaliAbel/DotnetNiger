using DotnetNiger.Identity.Application.Services.Interfaces;

namespace DotnetNiger.Identity.Application.Services;

public class EmailService : IEmailService
{
	// Placeholder: a remplacer par un vrai provider (SendGrid, SMTP, etc.).
	public Task SendAsync(string to, string subject, string body)
	{
		return Task.CompletedTask;
	}
}
