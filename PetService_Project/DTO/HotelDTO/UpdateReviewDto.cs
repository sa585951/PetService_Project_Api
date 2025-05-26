namespace PetService_Project_Api.DTO.HotelDTO
{
    public class UpdateReviewDto
    {
        public int? fOrderId { get; set; }
        public byte? Rating { get; set; }
        public string? Content { get; set; }
    }
}
