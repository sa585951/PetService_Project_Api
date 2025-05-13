namespace PetService_Project_Api.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string content);
        Task<bool> VerifyCodeAsync(string email, string code);
    }
}
