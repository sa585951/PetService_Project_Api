namespace PetService_Project_Api.DTO.HotelDTO
{
    public class HotelListPageDTO
    {
        public List<HotelListDto> Hotels { get; set; }
        public List<HotelItemDto> TotalItems { get; set; }
        public List<HotelSearchResponseDto> HotelDetailQty { get; set; }
        public List<ReviewResponseDTO> Review { get; set; }

    }
}
