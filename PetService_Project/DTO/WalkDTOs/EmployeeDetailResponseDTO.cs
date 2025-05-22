namespace PetService_Project_Api.DTO.WalkDTOs
{
    public class EmployeeDetailResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string District { get; set; }
        public int Price { get; set; }
        public List<string> PetTypes { get; set; }
        public List<string> PetSizes { get; set; } // 改為 string 以符合 "小型、中型" 的格式
        public int Distance { get; set; }
        public string DescriptionShort { get; set; }
        public string Description { get; set; }
        public List<string> Carousel { get; set; }//多張工作照
        public string EmployeeImage { get; set; }
        public string Map { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
