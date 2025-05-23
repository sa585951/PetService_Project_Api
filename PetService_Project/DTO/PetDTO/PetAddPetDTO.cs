namespace PetService_Project_Api.DTO.PetDTO
{
    public class PetAddPetDTO
    {
        public string PetName { get; set; }
        public int PetWeight { get; set; }
        public int? PetDe { get; set; }
        public DateOnly? PetBirthday { get; set; }
        public string? PetAvatarUrl { get; set; }
    }
}
