namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelSearchResponseDto
    {
        public int? Id { get; set; }
        public int? HotelId { get; set; }
        public int? SmallDogRoom { get; set; }
        public int? MiddleDogRoom { get; set; }
        public int? BigDogRoom { get; set; }
        public int? CatRoom { get; set; }
        public int RequiredRooms { get; set; } // 建議需要的房間數
    }
}
