using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.OrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;
using PetService_Project_Api.Service.Service;

namespace PetService_Project_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseController
    {
        protected readonly IOrderService _orderService;

        public OrderController(dbPetService_ProjectContext context,IOrderService orderService) : base(context)
        {
            _orderService = orderService;

            System.Diagnostics.Debug.Assert(context != null, "_context 注入失敗");
        }


        // GET: api/Order/
        [HttpGet]
        public async Task<ActionResult<OrderPagingResponseDTO>> GetOrders([FromQuery]OrdersSearchRequestDTO dto)
        {
            //1. 從JWT取得IdentityUser.Id
            
            var memberId = await GetMemberId();

            if (memberId ==null)
                return NotFound("找不到對應會員");

            //基本查詢
            var orders = _context.TOrders
                .Where(o => o.FMemberId == memberId)
                .AsQueryable();
            // keyword查詢訂單編號or建立時間
            if (!string.IsNullOrWhiteSpace(dto.keyword))
            {
                orders = orders.Where(o =>
                o.FId.ToString().Contains(dto.keyword) ||
                o.FCreatedAt.ToString().Contains(dto.keyword));
            }
            // 依ordertype過濾
            if (!string.IsNullOrWhiteSpace(dto.orderType) && dto.orderType != "all")
            {
                orders = orders.Where(o => o.FOrderStatus == dto.orderType);
            }
            // 排序
            if (!string.IsNullOrWhiteSpace(dto.sortBy))
            {
                switch(dto.sortBy)
                {
                    case "date_desc":
                        orders = orders.OrderByDescending(o => o.FCreatedAt);
                        break;
                    case "date_asc":
                        orders = orders.OrderBy(o => o.FCreatedAt);
                        break;
                    default:
                        orders = orders.OrderByDescending(o => o.FCreatedAt);
                        break;
                }
            }
            else
            {
                orders =orders.OrderByDescending(o => o.FCreatedAt);
            }

            //分頁計算
            int page = dto.page ?? 1;
            int pageSize = dto.pageSize ?? 10; //每頁10筆資料
            int total = await orders.CountAsync();
            int TotalPages = 1;
            if(pageSize > 0)
            {
                TotalPages = (int)Math.Ceiling(total / (double)pageSize);

                orders = orders.Skip((page - 1) * pageSize)
                                .Take(pageSize);
            }

            //包裝DTO回傳
            var result = new OrderPagingResponseDTO
            {
                TotalPages = TotalPages,
                OrdersResult = await orders.ToListAsync()
            };

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
                int OrderId =await _orderService.CreateWalkOrder(dto, memberId.ToString());
                return Ok(new { OrderId });
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

            var order = await _context.TOrders
                .FirstOrDefaultAsync(o =>
                o.FId == orderId &&
                o.FOrderType == "散步" &&
                o.FMemberId == memberId.Value);

            if (order == null)
                return NotFound("查無此訂單或無權限查看");

            var details = await _context.TOrderWalkDetails
                .Where(d => d.FOrderId == orderId)
                .Include(d => d.FEmployeeService)
                .ThenInclude(d => d.FEmployee)
                .ToListAsync();

            var result = new WalkOrderDetailResponseDTO
            {
                OrderId = order.FId,
                TotalAmount = (decimal)order.FTotalAmount,
                Status = order.FOrderStatus,
                CreatedAt = (DateTime)order.FCreatedAt,
                Items = details.Select(d => new WalkOrderItemResponseDTO
                {
                    EmployeeName = d.FEmployeeService.FEmployee.FName,
                    WalkStart = d.FWalkStart.Value,
                    WalkEnd = d.FWalkEnd.Value,
                    Amount = d.FAmount.Value,
                    ServicePrice = d.FServicePrice.Value,
                    TotalPrice = d.FTotalPrice.Value,
                    Note = d.FAdditionlＭessage
                }).ToList()
            };

            return Ok(result);
        }


    }
}
