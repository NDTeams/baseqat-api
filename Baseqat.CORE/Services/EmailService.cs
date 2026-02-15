using Baseqat.EF.Consts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Baseqat.CORE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            List<string> toEmails,
            string subject,
            string body,
            List<string>? ccEmails = null,
            List<string>? bccEmails = null,
            string? fromName = null)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPass = _configuration["EmailSettings:SmtpPass"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,

            };

            // Add To recipients
            foreach (var email in toEmails.Distinct())
            {
                mailMessage.To.Add(email);
            }

            // Add CC recipients
            if (ccEmails != null)
            {
                foreach (var cc in ccEmails.Distinct())
                {
                    mailMessage.CC.Add(cc);
                }
            }

            // Add BCC recipients
            if (bccEmails != null)
            {
                foreach (var bcc in bccEmails.Distinct())
                {
                    mailMessage.Bcc.Add(bcc);
                }
            }

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Optionally log the exception or rethrow
                throw new InvalidOperationException(ResponseMessages.EmailSendFailed, ex);
            }
        }
    }
}
