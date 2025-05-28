namespace PetService_Project_Api.DTO.WalkOrderDTOs
{
    public class WalkOrderDetailResponseDTO
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<WalkOrderItemResponseDTO> Items { get; set; }
    }

    public class WalkOrderItemResponseDTO
    {
        public string EmployeeName { get; set; }
        public DateTime WalkStart { get; set; }
        public DateTime WalkEnd { get; set; }
        public int Amount { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Note { get; set; }
        public string EmployeePhoto { get; set; }
    }
}
