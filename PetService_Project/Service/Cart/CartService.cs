using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;

namespace PetService_Project_Api.Service.Cart
{
    public class CartService : ICartService
    {
        private static readonly Dictionary<int ,List<WalkCartItemDTO>> _Walkcarts = new();
        private static readonly Dictionary<int,List<HotelCartItemDTO>> _Hotelcarts = new();

        public Task AddWalkItem(int memberId, WalkCartItemDTO item)
        {
            if(!_Walkcarts.ContainsKey(memberId))
            {
                _Walkcarts[memberId] = new List<WalkCartItemDTO>();
            }
            _Walkcarts[memberId].Add(item);

            return Task.CompletedTask;
        }

        public Task<List<WalkCartItemDTO>> GetWalkItems(int memberId)
        {
            var result = _Walkcarts.ContainsKey(memberId) ? _Walkcarts[memberId] : new List<WalkCartItemDTO>();

            return Task.FromResult(result);
        }

        public Task RemoveWalkItem(int memberId, int index)
        {
            if (_Walkcarts.ContainsKey(memberId) && index >= 0 && index < _Walkcarts[memberId].Count)
            {
                _Walkcarts[memberId].RemoveAt(index);
            }
            return Task.CompletedTask;
        }
        public Task ClearWalkCart(int memberId)
        {
            if (_Walkcarts.ContainsKey(memberId))
            {
                _Walkcarts[memberId].Clear();
            }
            return Task.CompletedTask;
        }

        public Task AddHotelItem(int memberId, HotelCartItemDTO item)
        {
            if (!_Hotelcarts.ContainsKey(memberId))
            {
                _Hotelcarts[memberId] = new List<HotelCartItemDTO>();
            }
            _Hotelcarts[memberId].Add(item);

            return Task.CompletedTask;

        }

        public Task<List<HotelCartItemDTO>> GetHotelItems(int memberId)
        {
            var result = _Hotelcarts.ContainsKey(memberId) ? _Hotelcarts[memberId] : new List<HotelCartItemDTO>();

            return Task.FromResult(result);
        }

        public Task RemoveHotelItem(int memberId, int index)
        {
            if(_Hotelcarts.ContainsKey(memberId) && index >= 0 && index < _Hotelcarts[memberId].Count)
            {
                _Hotelcarts[memberId].RemoveAt(index);
            }
            return Task.CompletedTask;
        }

        public Task ClearHotelItem(int memberId)
        {
            if(_Hotelcarts.ContainsKey(memberId))
            {
                _Hotelcarts[memberId].Clear();
            }
            return Task.CompletedTask;
        }
    }
}
