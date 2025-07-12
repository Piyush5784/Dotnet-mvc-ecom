using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using VMart.Interfaces;
using VMart.Models;
using VMart.Utility;

namespace VMart.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private readonly ILogService logEntry;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger, ILogService logEntry)
        {
            this.logEntry = logEntry;
            _configuration = configuration;
            _logger = logger;
        }


        // Explicit Identity UI method
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return SendMailAsync(email, subject, htmlMessage);
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
