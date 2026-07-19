using DotnetNiger.Domain.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Infrastructure.Data;

/// <summary>Service d'envoi d'email pour la communauté implémentant IEmailService.</summary>
public class CommunityEmailSender : EmailSenderBase, IEmailService
{
    /// <summary>Initialise le service avec les options SMTP.</summary>
    public CommunityEmailSender(IOptions<SmtpOptions> smtp, ILogger<CommunityEmailSender> logger)
        : base(smtp, logger) { }
}
