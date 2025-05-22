namespace PetService_Project_Api.DTO.OrderDTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public string OrderTypeCode { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal? TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
