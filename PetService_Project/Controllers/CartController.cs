using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetService_Project_Api.DTO.CartDTOs;
using PetService_Project_Api.Service.Cart;

namespace PetService_Project_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        private string GetMemberId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // GET: api/Cart/walk/add
        [HttpPost("walk/add")]
        public async Task<IActionResult> AddWalkItem([FromBody]WalkCartItemDTO dto)
        {
            var memberId = GetMemberId();
            await _cartService.AddWalkItem(memberId, dto);
            return Ok("已加入散步購物車");
        }

        // GET api/Cart/walk
        [HttpGet("walk")]
        public async Task<IActionResult> GetWalkItems()
        {
            var memberId = GetMemberId();
            var items = await _cartService.GetWalkItems(memberId);
            return Ok(items);
        }

        // DELETE api/Cart/walk/5
        [HttpDelete("walk/{index}")]
        public async Task<IActionResult> RemoveWalkItem(int index)
        {
            var memberId = GetMemberId();
            await _cartService.RemoveWalkItem(memberId, index);
            return Ok("已移除指定項目");
        }

        // DELETE api/Cart/walk/clear
        [HttpDelete("walk/clear")]
        public async Task<IActionResult> ClearWalkItem()
        {
            var memberId = GetMemberId();
            await _cartService.ClearCart(memberId);
            return Ok("已清除購物車");
        }

    }
}
