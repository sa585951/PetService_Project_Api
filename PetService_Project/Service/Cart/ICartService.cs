using PetService_Project_Api.DTO.CartDTOs;

namespace PetService_Project_Api.Service.Cart
{
    public interface ICartService
    {
        Task AddWalkItem(string memberId, WalkCartItemDTO item);
        Task<List<WalkCartItemDTO>> GetWalkItems(string memberId);
        Task RemoveWalkItem(string memberId, int index);
        Task ClearCart(string memberId);
    }
}
