using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.OrderDTOs;
using PetService_Project_Api.DTO.PaymentDTO;
using PetService_Project_Api.DTO.WalkOrderDTOs;
using PetService_Project_Api.Service.OrderEmail;
using PetService_Project_Api.Service.Payment;
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
        protected readonly  IEcpayService _ecpayService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(dbPetService_ProjectContext context,IOrderService orderService,IOrderNotificationEmailService emailService,IEcpayService ecpayService,ILogger<OrderController> logger
            ) : base(context)
        {
            _orderService = orderService;
            _emailService = emailService;
            _ecpayService = ecpayService;
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

        //軟刪除 for 後台管理 
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
        //取消訂單api
        [HttpPatch("{orderId}/cancel")]
        public async Task<ActionResult> CancelOrder(int orderId)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return NotFound("找不到對應會員");
            await _orderService.UpdateOrderStatusAsync(memberId.Value, orderId, "已取消");
            return NoContent();
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

        [HttpPost("{orderId}/ecpay-checkout")]
        public async Task<IActionResult> Checkout(int orderId)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return Unauthorized("找不到會員");

            var order = await _context.TOrders.FirstOrDefaultAsync(o=>o.FId == orderId);
            if (order == null)
                return NotFound("查無此訂單");

            if (order.FOrderStatus != "未付款")
                return BadRequest("此訂單無法付款");

            var html = await _ecpayService.GenerateCheckoutHtmlAsync(
                merchantTradeNo: order.FmerchantTradeNo,
                amount: (decimal)order.FTotalAmount,
                itemName: $"訂單編號#{order.FId}"
                );

            return Content(html, "text/html");
        }

        [AllowAnonymous]
        [HttpPost("ecpay-callback")]
        public async Task<IActionResult> EcpayCallback()
        {
            var form = Request.Form;
            var isSuccess = await _ecpayService.ProcessCallbackAsync(form);
            return Content("1|OK");
        }

        [HttpGet("{orderId}/payment-info")]
        public async Task<IActionResult> GetOrderPaymentInfo(int orderId)
        {
            var memberId = await GetMemberId();
            if (memberId == null)
                return Unauthorized("找不到會員");

            //查詢訂單
            var order = await _context.TOrders.FirstOrDefaultAsync(o=>o.FId== orderId);

            if(order == null) 
                return NotFound("查無此訂單");

            if (order.FOrderStatus != "未付款")
                return BadRequest("此訂單無須付款");

            var dto = new
            {
                MerchantTradeNo = order.FmerchantTradeNo,
                TotalAmount = order.FTotalAmount,
                ItemName = $"訂單編號#{order.FId}"
            };

            return Ok(dto);
        }
    }
}
