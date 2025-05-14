namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Longitude { get; set; } //經度 詳細才要
        public string Latitude { get; set; } //緯度 詳細才要
        public string Image_1 { get; set; }
        public string Image_2 { get; set; } //詳細才要
        public string Image_3 { get; set; } //詳細才要

    }
}
