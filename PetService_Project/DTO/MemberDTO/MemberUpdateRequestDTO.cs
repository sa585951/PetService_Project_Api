namespace PetService_Project_Api.DTO.MemberDTO
{
    public class MemberUpdateRequestDTO
    {
        public string Phone { get; set; }
        public string Address { get; set; }
        public string AvatarUrl { get; set; } // 這是使用者自己上傳的新圖
    }
}
