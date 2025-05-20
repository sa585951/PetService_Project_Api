using SendGrid.Helpers.Mail;
using SendGrid;

namespace PetService_Project_Api.Service
{
    public class SendGridService:IEmailService
    {
        private readonly string _apiKey;
        private readonly ICodeService _codeCache;

        public SendGridService(IConfiguration configuration, ICodeService codecache)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _codeCache = codecache;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress("fuen41t2@gmail.com", "毛孩管家");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"SendGrid response status: {response.StatusCode}");
        }

        // 驗證驗證碼是否正確
        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var cachedCode = await _codeCache.GetCodeAsync(email);
            return cachedCode == code;
        }
    }
}
