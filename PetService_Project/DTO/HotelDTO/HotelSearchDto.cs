namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelSearchDto
    {
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int PetCount { get; set; }
        public int Type { get; set; }
        public int? HotelId { get; set; }
    }
}