using Application.Interfaces.Integrations;
using Application.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Shared.Models;
using System;


namespace Infrastructure.ExtenalServices
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value ?? throw new ArgumentNullException(nameof(smtpSettings));
        }
        public async Task SendEmailAsync(EmailRequest request)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);
            email.To.Add(MailboxAddress.Parse(request.ToEmail));
            email.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = request.Body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            // connect to the SMTP server
            await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

            // Authenticate with the SMTP server
            await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

            // Send the email
            await smtp.SendAsync(email);

            // Disconnect from the SMTP server
            await smtp.DisconnectAsync(true);
        }
    }
}
