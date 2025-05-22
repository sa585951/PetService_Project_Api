using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
namespace PetService_Project.Partials
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string SenderEmail { get; set; } = "";
    }
}