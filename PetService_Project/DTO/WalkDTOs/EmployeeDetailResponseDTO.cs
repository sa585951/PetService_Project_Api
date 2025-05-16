namespace PetService_Project_Api.DTO.WalkDTOs
{
    public class EmployeeDetailResponseDTO
    {
        public string Name { get; set; }
        public string District { get; set; }
        public int Price { get; set; }
        public List<string> PetTypes { get; set; }
        public List<string> PetSizes { get; set; }
        public int Distance { get; set; }
        public string DescriptionShort { get; set; }
        public string Description { get; set; }
        public List<string> Photos { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
