using Application.Interfaces.Integrations;
using Application.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Shared.Common.QRCode;
using Shared.Models;
using Shared.Models.ExtenalModels;
using Shared.Templates;
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

        public async Task SendBookingConfirmationEmailAsync(string toEmail, EmailConfirmBookingResponse bookingInfo)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);
            email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = BookingConfirmationTemplate.BOOKING_CONFIRMATION_SUBJECT;

            // Generate QR Code
            byte[] qrCodeBytes = QrCodeHelper.GenerateQrCodeBytes(bookingInfo.BookingCode.ToString());

            // Create body builder
            var builder = new BodyBuilder();

            // Add QR Code as inline attachment
            var qrImage = builder.LinkedResources.Add("qrcode.png", qrCodeBytes, ContentType.Parse("image/png"));
            qrImage.ContentId = "qrcode"; // Set ContentId for referencing in HTML

            // Set HTML body with QR code reference
            builder.HtmlBody = BookingConfirmationTemplate.BookingConfirmation(bookingInfo);

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            try
            {
                // Connect to the SMTP server
                await smtp.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);

                // Authenticate with the SMTP server
                await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

                // Send the email
                await smtp.SendAsync(email);
            }
            finally
            {
                // Disconnect from the SMTP server
                await smtp.DisconnectAsync(true);
            }
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
