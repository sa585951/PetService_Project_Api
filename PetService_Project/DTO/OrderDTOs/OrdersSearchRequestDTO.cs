namespace PetService_Project_Api.DTO.OrderDTOs
{
    public class OrdersSearchRequestDTO
    {
        public string? keyword { get; set; }
        public string? orderType { get; set; } = "all";
        public string? sortBy { get; set; }
        public int? page { get; set; } = 1;
        public int? pageSize { get; set; } = 9;
    }
}
