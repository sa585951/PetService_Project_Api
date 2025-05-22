
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using PetService_Project.Partials;

namespace PetService_Project_Api.Service.OrderEmail
{
    public class SmtpEmailService : IOrderNotificationEmailService
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailService(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task SendEmailAsync(string to, string subject, string content)
        {
            var mail = new MailMessage()
            {
                From = new MailAddress(_opt.SenderEmail, _opt.SenderName),
                Subject = subject,
                Body = content,
                IsBodyHtml = false
            };
            mail.To.Add(to);

            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl,
                Credentials = new NetworkCredential(_opt.User, _opt.Password)
            };
            await client.SendMailAsync(mail);
        }

    }
}
