namespace PetService_Project_Api.DTO.HotelDTO
{
    public class CreateReviewDto
    {
        public int? HotelId { get; set; }
        public int? RoomtypeId { get; set; }
        public int? MemberId { get; set; }
        public int? OrderId { get; set; }
        public byte? Rating { get; set; }
        public string? Content { get; set; }
    }
}
