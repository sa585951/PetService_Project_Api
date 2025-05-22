namespace PetService_Project_Api.Service.OrderEmail
{
    public interface IOrderNotificationEmailService
    {
        Task SendEmailAsync(string to, string subject, string content);
    }
}
