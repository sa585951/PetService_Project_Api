namespace PetService_Project_Api.DTO
{
    public class AccountResetPasswordRequestDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
