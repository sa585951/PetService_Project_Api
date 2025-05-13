namespace PetService_Project_Api.DTO
{
    public class AccountVerifyEmailCodeRequestDTO
    {
        public string Email { get; set; }
        public string Code { get; set; }  // 新增 Code 欄位
    }
}
