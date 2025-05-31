namespace PetService_Project_Api.DTO.HotelOrderDTOs
{
    public class HotelOrderDetailResponseDTO
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<HotelOrderItemResponseDTO> Items { get; set; }
    }

    public class HotelOrderItemResponseDTO
    {
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Qty { get; set; }
        public decimal PricePerRoom { get; set; }
        public decimal TotalPrice { get; set; }
        public string Note { get; set; }
        public int Nights { get; set; }
        public string HotelPhoto { get; set; }
    }
}
