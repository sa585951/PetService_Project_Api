namespace PetService_Project_Api.DTO.MemberDTO
{
    public class AccountForgotPasswordRequestDTO
    {
        public string Email { get; set; }
        public string FrontendUrl { get; set; } // 前端用來放置 reset-password 頁面的網址
    }
}
