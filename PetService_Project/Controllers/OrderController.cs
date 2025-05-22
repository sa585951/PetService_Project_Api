using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.OrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;
using PetService_Project_Api.Service.OrderEmail;
using PetService_Project_Api.Service.Service;
using StackExchange.Redis;

namespace PetService_Project_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseController
    {
        protected readonly IOrderService _orderService;
        protected readonly IOrderNotificationEmailService _emailService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(dbPetService_ProjectContext context,IOrderService orderService,IOrderNotificationEmailService emailService,ILogger<OrderController> logger
            ) : base(context)
        {
            _orderService = orderService;
            _emailService = emailService;
            _logger = logger;
            System.Diagnostics.Debug.Assert(context != null, "_context 注入失敗");
        }


        // GET: api/Order/
        [HttpGet]
        public async Task<ActionResult<OrderPagingResponseDTO>> GetOrderAsync( [FromQuery]OrdersSearchRequestDTO dto)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到會員");

            var result = await _orderService.GetOrderAsync(memberId.Value, dto);
            return Ok(result);
        }

        [HttpPost("walk/create")]
        public async Task<ActionResult> CreateWalkOrder([FromBody] CreateWalkOrderRequestDTO dto)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到對應會員");

            try
            {
                var id = await _orderService.CreateWalkOrder(memberId.Value, dto);

                var member = await _context.TMembers.FindAsync(memberId);
                if(member != null)
                {
                    try
                    {
                        var content = $"親愛的會員您好，您的訂單已成功建立，編號為 #100{id}。感謝您的使用!";
                        await _emailService.SendEmailAsync(member.FEmail, "訂單成立通知", content);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"寄送訂單通知失敗會員{member.FEmail}:{ex.Message}");
                    }
                    
                }
                return Ok(new { OrderId = id });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"建立訂單時發生錯誤:{ex.Message}");
            }


        }

        [HttpGet("walk/{orderId}")]
        public async Task<IActionResult> GetWalkOrderDetail(int orderId)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到對應會員");
            var result = await _orderService.GetWalkOrderDetailAsync(memberId.Value, orderId);         
            return Ok(result);
        }

        [HttpPost("hotel/create")]
        public async Task<ActionResult> CreateHotelOrder([FromBody] CreateHotelOrderRequestDTO dto)
        {
            Console.WriteLine("➡️ 有打進 CreateHotelOrder");
            _logger.LogInformation("➡️ 有打進 CreateHotelOrder");

            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到對應會員");

            try
            {
                var id = await _orderService.CreateHotelOrder(memberId.Value, dto);
                var member = await _context.TMembers.FindAsync(memberId);

                if (member != null)
                {
                    try
                    {
                        var content = $"親愛的會員您好，您的訂單已成功建立，編號為 #100{id}。感謝您的使用!";
                        await _emailService.SendEmailAsync(member.FEmail, "訂單成立通知", content);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"寄送訂單通知失敗會員{member.FEmail}:{ex.Message}");
                    }
                }
                return Ok(new
                { OrderId= id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"建立訂單發生錯誤:{ex.Message}");
            }
        }

        [HttpGet("hotel/{orderId}")]
        public async Task<IActionResult> GetHotelOrderDetail(int orderId)
        {
            var memberId = await GetMemberId();

            if (memberId == null)
                return NotFound("找不到對應會員");
            var result = await _orderService.GetHotelOrderDetailAsync(memberId.Value, orderId);
            return Ok(result);
        }

        [HttpPatch("{orderId}/soft-delete")]
        public async Task<ActionResult> SoftDelete(int orderId)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到對應會員");
            try
            {
                await _orderService.SoftDeleteOrderAsync(memberId.Value, orderId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("查無此訂單或無權限刪除");
            }
        }

        [HttpPatch("{orderId}/status")]
        public async Task<ActionResult> ChangeStatus(int orderId, [FromBody]UpdateOrderStatusDTO dto)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到對應會員");
            await _orderService.UpdateOrderStatusAsync(memberId.Value, orderId, dto.OrderStatus);
            return NoContent();
        }
    }
}
