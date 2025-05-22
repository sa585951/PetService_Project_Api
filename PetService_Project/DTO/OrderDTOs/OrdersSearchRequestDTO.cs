namespace PetService_Project_Api.DTO.OrderDTOs
{
    public class OrdersSearchRequestDTO
    {
        public string? keyword { get; set; }
        public string? orderType { get; set; } = "all";  //"walk" or "hotel" 
        public string? orderStatus { get; set; }//付款狀態
        public string? sortBy { get; set; }
        public int? page { get; set; } = 1;
        public int? pageSize { get; set; } = 9;
    }
}
