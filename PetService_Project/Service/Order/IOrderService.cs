using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.OrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;

namespace PetService_Project_Api.Service.Service
{
    public interface IOrderService
    {
        Task<OrderPagingResponseDTO> GetOrderAsync(int memberId, OrdersSearchRequestDTO dto);
        Task<int> CreateWalkOrder(int memberId, CreateWalkOrderRequestDTO dto);
        Task<int> CreateHotelOrder( int memberId,CreateHotelOrderRequestDTO dto);
        Task SoftDeleteOrderAsync(int memberId,int orderId);
        Task UpdateOrderStatusAsync(int memberId, int orderId, string newStatus);

        Task<WalkOrderDetailResponseDTO> GetWalkOrderDetailAsync(int memberId, int orderId);
        Task<HotelOrderDetailResponseDTO> GetHotelOrderDetailAsync(int memberId, int orderId);
    }
}
