using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.ProjectSettings;
using Infrastructure.Email;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings settings;
        private readonly AuthMessageSenderOptions authMessageSenderOptions;

        public EmailSender(IOptions<SmtpSettings> stmpSettings, IOptions<AuthMessageSenderOptions> authMessageSenderOptions)
        {
            settings = stmpSettings.Value;
            this.authMessageSenderOptions = authMessageSenderOptions.Value;
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
        //public async Task SendAuthEmailAsync(string toEmail, string subject, string message)
        //{
        //    await Execute(subject, message, toEmail);
        //}

        //public async Task Execute(string subject, string message, string toEmail)
        //{
        //    var client = new SendGridClient(apiKey);
        //    var msg = new SendGridMessage()
        //    {
        //        From = new EmailAddress("PowerOutagesNI@outlook.com", "Password Recovery"),
        //        Subject = subject,
        //        PlainTextContent = message,
        //        HtmlContent = message
        //    };
        //    msg.AddTo(new EmailAddress(toEmail));

        //    // Disable click tracking.
        //    // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        //    msg.SetClickTracking(false, false);
        //    var response = await client.SendEmailAsync(msg);
        //}
    }
}
