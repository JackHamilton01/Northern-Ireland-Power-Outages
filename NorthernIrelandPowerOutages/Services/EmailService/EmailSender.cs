using Infrastructure.Email;
using Infrastructure.ProjectSettings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings settings;

        public EmailSender(IOptions<SmtpSettings> options)
        {
            settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(settings.SourceEmail, "Power Outages NI"),
                Subject = subject,
                Body = message,
                IsBodyHtml = false
            };

            mailMessage.To.Add(new MailAddress(email));

            using var smtpClient = new SmtpClient(settings.Server, settings.Port)
            {
                Credentials = new NetworkCredential(settings.SourceEmail, settings.SourceEmailPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
