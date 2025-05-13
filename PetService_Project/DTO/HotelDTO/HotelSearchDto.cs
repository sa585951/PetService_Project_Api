namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelSearchDto
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int PetCount { get; set; }
        public List<string>? Amenity { get; set; }  // ex: 設施
        public List<string>? Service { get; set; }   // ex: 服務
    }
}