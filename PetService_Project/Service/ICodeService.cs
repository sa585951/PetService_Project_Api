namespace PetService_Project_Api.Service
{
    public interface ICodeService
    {
        Task SetCodeAsync(string email, string code, TimeSpan expiration);
        Task<string> GetCodeAsync(string email);
    }
}
