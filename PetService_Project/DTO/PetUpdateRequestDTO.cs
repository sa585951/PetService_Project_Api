namespace PetService_Project_Api.DTO
{
    public class PetUpdateRequestDTO
    {
        public int Id { get; set; }
        public string PetName { get; set; }
        public int PetWeight { get; set; }
        public int PetAge { get; set; }
        public int PetDe { get; set; }
        public DateOnly PetBirthday { get; set; }
        public string PetAvatarUrl { get; set; } 
    }
}
