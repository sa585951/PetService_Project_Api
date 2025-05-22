namespace PetService_Project_Api.DTO.HotelOrderDTOs
{
    public class HotelCartItemDTO
    {
        public int HotelId { get; set; }
        public int RoomDetailId { get; set; }
        public DateTime CheckIn {  get; set; }
        public DateTime CheckOut { get; set; }
        public int RoomQty { get; set; }
        public string? AdditionalMessage { get; set; }
    }
}
