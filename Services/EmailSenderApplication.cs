using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mail;
using VMart.Data;
using VMart.Interfaces;
using VMart.Models;
using VMart.Utility;

namespace VMart.Services
{
    public class EmailSenderApplication : IEmailSenderApplicationInterface
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly ILogService logEntry;

        public EmailSenderApplication(IConfiguration config, ILogger<EmailSender> _logger, ILogService logEntry)
        {
            this._configuration = config;
            this._logger = _logger;
            this.logEntry = logEntry;
        }

        public Task SendContactMessageToAdminAsync(Contact contact)
        {
            var adminEmail = _configuration["EmailSettings:AdminEmail"]!;
            var subject = $"New Contact Message from {contact.FullName}";
            var body = $@"
        <html>
          <body>
            <h3>New Contact Submission</h3>
            <p><strong>Name:</strong> {contact.FullName}</p>
            <p><strong>Email:</strong> {contact.Email}</p>
            <p><strong>Phone:</strong> {contact.PhoneNumber}</p>
            <p><strong>Message:</strong><br/>{contact.Message}</p>
          </body>
        </html>";

            return SendMailAsync(adminEmail, subject, body);
        }


        private async Task SendMailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // Read settings safely
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration.GetValue("EmailSettings:SmtpPort", 587);
                var fromEmail = _configuration["EmailSettings:FromEmail"]!;
                var fromPass = _configuration["EmailSettings:FromPassword"];
                var displayName = _configuration["EmailSettings:DisplayName"] ?? "E-Commerce";

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(fromEmail, fromPass),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, displayName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                await logEntry.LogAsync(
                      SD.Log_Error,
                      "Email sending failed",
                      "EmailSender Application",
                      "action",
                       ex.ToString(),
                       $"To: {toEmail}, Subject: {subject}",
                       toEmail
                       );
            }
        }

    }
}
