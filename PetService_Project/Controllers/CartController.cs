using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;
using PetService_Project_Api.Service.Cart;

namespace PetService_Project_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        public CartController(dbPetService_ProjectContext context,ICartService cartService): base(context) 
        {
            _cartService = cartService;
        }

        // POST: api/Cart/walk/add
        [HttpPost("walk/add")]
        public async Task<IActionResult> AddWalkItem([FromBody]WalkCartItemDTO dto)
        {
            var memberId = await GetMemberId();
            await _cartService.AddWalkItem(memberId.Value, dto);
            return Ok("已加入散步購物車");
        }

        // GET api/Cart/walk
        [HttpGet("walk")]
        public async Task<IActionResult> GetWalkItems()
        {
            var memberId = await GetMemberId();
            var items = await _cartService.GetWalkItems(memberId.Value);
            return Ok(items);
        }

        // DELETE api/Cart/walk/5
        [HttpDelete("walk/{index}")]
        public async Task<IActionResult> RemoveWalkItem(int index)
        {
            var memberId = await GetMemberId();
            await _cartService.RemoveWalkItem(memberId.Value, index);
            return Ok("已移除指定項目");
        }

        // DELETE api/Cart/walk/clear
        [HttpDelete("walk/clear")]
        public async Task<IActionResult> ClearWalkItem()
        {
            var memberId = await GetMemberId();
            await _cartService.ClearWalkCart(memberId.Value);
            return Ok("已清除購物車");
        }

        //POST: api/cart/hotel/add
        [HttpPost("hotel/add")]
        public async Task<IActionResult> AddHotelItem([FromBody]HotelCartItemDTO dto)
        {
            var memberId = await GetMemberId();
            await _cartService.AddHotelItem(memberId.Value, dto);
            return Ok("已加入住宿購物車");
        }

        // GET api/Cart/hotel
        [HttpGet("hotel")]
        public async Task<IActionResult> GetHotelItems()
        {
            var memberId = await GetMemberId();
            var items = await _cartService.GetHotelItems(memberId.Value);
            return Ok(items);
        }

        // DELETE api/Cart/hotel/5
        [HttpDelete("hotel/{index}")]
        public async Task<IActionResult> RemoveHotelItem(int index)
        {
            var memberId = await GetMemberId();
            await _cartService.RemoveHotelItem(memberId.Value, index);
            return Ok("已移除指定項目");
        }

        // DELETE api/Cart/walk/clear
        [HttpDelete("hotel/clear")]
        public async Task<IActionResult> ClearHotelItem()
        {
            var memberId = await GetMemberId();
            await _cartService.ClearHotelItem(memberId.Value);
            return Ok("已清除購物車");
        }
    }
}
