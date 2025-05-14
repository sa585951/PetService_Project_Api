namespace PetService_Project_Api.DTO
{
    public class PetDataRequestDTO
    {
        public int MemberId { get; set; }
        public string? PetName { get; set; } // 使用 ? 表示可為 null
        public int? PetWeight { get; set; }
        public int? PetAge { get; set; }
        // ... 其他您想要返回給前端的寵物屬性 ...
        public DateTime? PetBirthday { get; set; } // 日期類型使用 DateTime 或 DateOnly
        public string? PetImagePath { get; set; }
        // 您可能不需要返回所有 fPetDe, fPetTrained 等欄位，只返回前端需要的

    }
}
