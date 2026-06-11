using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Booking.API.Notifications.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtp = _options.Smtp;
        if (!IsConfigured(smtp))
        {
            _logger.LogWarning(
                "SMTP email is not configured. Skipping email to {Recipient} with subject {Subject}",
                message.To,
                message.Subject);
            return;
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(smtp.From, smtp.FromName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };
        mailMessage.To.Add(message.To);

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(smtp.Username))
        {
            client.Credentials = new NetworkCredential(smtp.Username, smtp.Password);
        }

        await client.SendMailAsync(mailMessage, cancellationToken);
    }

    private static bool IsConfigured(SmtpOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.Host) &&
               options.Port > 0 &&
               !string.IsNullOrWhiteSpace(options.From);
    }
}
