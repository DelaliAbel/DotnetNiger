using System.Net;
using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Expédie les courriels par SMTP.
/// </summary>
/// <remarks>
/// Utilise MailKit : <c>System.Net.Mail.SmtpClient</c> est marqué obsolète par .NET,
/// qui recommande explicitement cette bibliothèque. Tant qu'aucun SMTP n'est configuré,
/// le message est écrit dans le journal : en développement local, le parcours
/// d'inscription reste donc testable de bout en bout.
/// </remarks>
public sealed partial class SmtpEmailSender(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    /// <inheritdoc />
    public async Task SendAsync(
        string recipient,
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.IsConfigured)
        {
            LogSansServeur(recipient, message.Subject, message.HtmlBody);
            return;
        }

        var courriel = new MimeMessage();
        courriel.From.Add(new MailboxAddress(_options.FromDisplayName, _options.FromAddress));
        courriel.To.Add(MailboxAddress.Parse(recipient));
        courriel.Subject = message.Subject;
        courriel.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        if (!string.IsNullOrWhiteSpace(_options.ReplyToAddress))
        {
            courriel.ReplyTo.Add(MailboxAddress.Parse(_options.ReplyToAddress));
        }

        using var client = new SmtpClient();
        var securite = _options.UseSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None;

        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, securite, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                // Certains relais refusent l'authentification tant que STARTTLS n'a pas été
                // négocié. Si « UseSsl » est désactivé alors qu'il le faudrait, l'erreur du
                // serveur nomme la cause : elle est journalisée telle quelle plutôt que
                // masquée derrière un message plus générique.
                await client.AuthenticateAsync(
                    _options.UserName,
                    _options.Password ?? string.Empty,
                    cancellationToken);
            }

            await client.SendAsync(courriel, cancellationToken);
        }
        catch (SmtpCommandException exception)
        {
            // Traduite en erreur du site : l'appelant n'a pas à savoir quel serveur SMTP a
            // refusé le message, mais il doit apprendre que le message n'est pas parti.
            throw new EmailDeliveryException(
                $"Le serveur {_options.Host} a refusé le message pour {recipient} : {exception.Message}",
                exception);
        }
        catch (SmtpProtocolException exception)
        {
            throw new EmailDeliveryException(
                $"Le serveur {_options.Host} n'a pas répondu dans un délai acceptable pour {recipient}.",
                exception);
        }
        catch (AuthenticationException exception)
        {
            throw new EmailDeliveryException(
                $"Les identifiants SMTP sont refusés par {_options.Host}.",
                exception);
        }
        catch (SocketException exception)
        {
            throw new EmailDeliveryException(
                $"Le serveur SMTP {_options.Host} est injoignable.", exception);
        }

        LogEnvoye(logger, recipient, message.Subject);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Courriel « {Subject} » envoyé à {Recipient}.")]
    private static partial void LogEnvoye(ILogger logger, string recipient, string subject);

    private void LogSansServeur(string recipient, string subject, string htmlBody)
    {
        LogSansServeurServeur(logger, recipient, subject, htmlBody);
    }

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Aucun serveur SMTP configuré : le message n'est pas envoyé. Destinataire {Recipient}, objet {Subject}. Corps : {Body}")]
    private static partial void LogSansServeurServeur(
        ILogger logger,
        string recipient,
        string subject,
        string body);
}
