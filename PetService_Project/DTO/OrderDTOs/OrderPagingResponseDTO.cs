using PetService_Project.Models;
using PetService_Project_Api.Models;

namespace PetService_Project_Api.DTO.OrderDTOs
{
    public class OrderPagingResponseDTO
    {
        public int TotalPages { get; set; }

        public List<OrderDTO> OrdersResult { get; set; }

    }
}
