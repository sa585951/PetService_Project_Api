namespace PetService_Project_Api.DTO.HotelDTO
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public string? Roomtype { get; set; }
        public string? MemberName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte? Rating { get; set; }
        public string? Content { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
