namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelSearchResponseDto
    {
        public List<int>? SearchedHotelId { get; set; }
        public int? SmallDogRoom { get; set; }
        public int? MiddleDogRoom { get; set; }
        public int? BigDogRoom { get; set; }
        public int? CatRoom { get; set; }
    }
}
