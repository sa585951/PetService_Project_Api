using PetService_Project_Api.DTO.WalkOrderDTOs;

namespace PetService_Project_Api.Service.Service
{
    public interface IOrderService
    {
        Task<int> CreateWalkOrder(CreateWalkOrderRequestDTO dto, string memberId);
    }
}
