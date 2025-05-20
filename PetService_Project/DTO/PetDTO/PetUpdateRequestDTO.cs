namespace PetService_Project_Api.DTO.PetDTO
{
    public class PetUpdateRequestDTO
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public int? PetDelete { get; set; }
        public int PetWeight { get; set; }
        public int? PetDe { get; set; }
        public DateTime? PetBirthday { get; set; }
        public string? PetAvatarUrl { get; set; } 
    }
}
