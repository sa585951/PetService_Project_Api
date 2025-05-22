using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;

namespace PetService_Project_Api.Service.Cart
{
    public interface ICartService
    {
        //散步行程
        Task AddWalkItem(int memberId, WalkCartItemDTO item);
        Task<List<WalkCartItemDTO>> GetWalkItems(int memberId);
        Task RemoveWalkItem(int memberId, int index);
        Task ClearWalkCart(int memberId);

        //飯店訂房
        Task AddHotelItem(int memberId, HotelCartItemDTO item);
        Task<List<HotelCartItemDTO>> GetHotelItems(int memberId);
        Task RemoveHotelItem(int memberId, int index);
        Task ClearHotelItem(int memberId);
    }
}
