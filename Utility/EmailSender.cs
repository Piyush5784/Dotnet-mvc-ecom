using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using VMart.Models;

namespace VMart.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task SendContactMessageToAdminAsync(Contact contact)
        {
            var adminEmail = _configuration["EmailSettings:AdminEmail"];
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


        // Explicit Identity UI method
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return SendMailAsync(email, subject, htmlMessage);
        }

        // Optional helpers for common flows:
        public Task SendConfirmationLinkAsync(IdentityUser user, string confirmationLink)
        {
            var subject = "Confirm your email";
            var body = $@"
                <html>
                  <body>
                    <h2>Welcome to E-Commerce!</h2>
                    <p>Hello {user.UserName},</p>
                    <p>Confirm your email by clicking below:</p>
                    <p>
                      <a href='{confirmationLink}'
                         style='background:#4CAF50;color:white;padding:10px 20px;
                                text-decoration:none;border-radius:5px;'>
                        Confirm Email
                      </a>
                    </p>
                    <p>If you didn't create an account, ignore this email.</p>
                    <p>Regards,<br/>E-Commerce Team</p>
                  </body>
                </html>";
            return SendMailAsync(user.Email, subject, body);
        }

        public Task SendPasswordResetLinkAsync(IdentityUser user, string resetLink)
        {
            var subject = "Reset your password";
            var body = $@"
                <html>
                  <body>
                    <h2>Password Reset</h2>
                    <p>Hello {user.UserName},</p>
                    <p>Reset it by clicking below:</p>
                    <p>
                      <a href='{resetLink}'
                         style='background:#f44336;color:white;padding:10px 20px;
                                text-decoration:none;border-radius:5px;'>
                        Reset Password
                      </a>
                    </p>
                    <p>If you didn't request this, ignore this email.</p>
                    <p>Regards,<br/>E-Commerce Team</p>
                  </body>
                </html>";
            return SendMailAsync(user.Email, subject, body);
        }

        private async Task SendMailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // Read settings safely
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 587);
                var fromEmail = _configuration["EmailSettings:FromEmail"];
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
#if DEBUG
                throw;
#endif
            }
        }
    }
}
