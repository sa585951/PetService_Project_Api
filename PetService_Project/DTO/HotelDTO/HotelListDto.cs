using PetService_Project.Models;

namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelListDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public decimal? Longitude { get; set; } //經度 詳細才要
        public decimal? Latitude { get; set; } //緯度 詳細才要
        public string? Image_1 { get; set; }
        public string? Image_2 { get; set; } //詳細才要
        public string? Image_3 { get; set; } //詳細才要
        public byte? Rating { get; set; } //詳細才要

        public List<RoomTypeDto> RoomTypes { get; set; }
        public List<HotelItemDto> Items { get; set; }
        public List<RoomDetailDto> RoomDetail { get; set; }
        public List<RoomQtyStatus> QtyStatus { get; set; }
    }

    public class RoomDetailDto
    {
        public int Id { get; set; }
        public int? Roomtype_id { get; set; }
        public int? Price { get; set; }
        public string? Image { get; set; }
        public string? Roomsize { get; set; }
    }

    public class RoomTypeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class HotelItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }   // 設施或服務
        public string? Description { get; set; } //敘述
    }
    public class RoomQtyStatus
    {
        public int? Id { get; set; }
        public int? HotelId { get; set; }
        public int? SmallDogRoom { get; set; }
        public int? MiddleDogRoom { get; set; }
        public int? BigDogRoom { get; set; }
        public int? CatRoom { get; set; }
    }
}
