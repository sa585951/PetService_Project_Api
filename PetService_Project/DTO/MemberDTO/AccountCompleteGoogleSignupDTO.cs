namespace PetService_Project_Api.DTO.MemberDTO
{
    public class AccountCompleteGoogleSignupDTO
    {
        // 手機號碼 (假設是字串)
        public string? Phone { get; set; } // 使用可為 Null 的字串以符合前端可能傳空的情況，後端再驗證是否必填

        // 地址 (假設是字串)
        public string? Address { get; set; } // 使用可為 Null 的字串

        // 選擇的資訊來源 ID 列表 (假設 ID 是整數)
        public List<int> Sources { get; set; }
    }
}
