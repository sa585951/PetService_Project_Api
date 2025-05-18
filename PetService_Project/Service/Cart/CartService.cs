using PetService_Project_Api.DTO.CartDTOs;

namespace PetService_Project_Api.Service.Cart
{
    public class CartService : ICartService
    {
        private static readonly Dictionary<string ,List<WalkCartItemDTO>> _carts = new();

        public Task AddWalkItem(string memberId, WalkCartItemDTO item)
        {
            if(!_carts.ContainsKey(memberId))
            {
                _carts[memberId] = new List<WalkCartItemDTO>();
            }
            _carts[memberId].Add(item);

            return Task.CompletedTask;
        }

        public Task<List<WalkCartItemDTO>> GetWalkItems(string memberId)
        {
            var result =  _carts.ContainsKey(memberId) ? _carts[memberId] : new List<WalkCartItemDTO>();

            return Task.FromResult(result);
        }

        public Task RemoveWalkItem(string memberId, int index)
        {
            if (_carts.ContainsKey(memberId) && index >= 0 && index < _carts[memberId].Count)
            {
                _carts[memberId].RemoveAt(index);
            }
            return Task.CompletedTask;
        }
        public Task ClearCart(string memberId)
        {
            if (_carts.ContainsKey(memberId))
            {
                _carts[memberId].Clear();
            }
            return Task.CompletedTask;
        }
    }
}
