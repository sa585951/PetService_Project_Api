using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.OrderDTOs;

namespace PetService_Project_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public OrderController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        // GET: api/Order/members
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<OrdersPagingDTO>> GetOrders([FromQuery]OrdersSearchDTO dto)
        {
            //1. 從JWT取得IdentityUser.Id
            string aspNetUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(aspNetUserId))
                return Unauthorized("無法取得會員ID");
            //2.從資料庫查出MemberId
            var member = await _context.TMembers.FirstOrDefaultAsync(m => m.FAspNetUserId == aspNetUserId);

            if (member ==null)
                return NotFound("找不到對應會員");

            int memberId = member.FId;

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
            var result = new OrdersPagingDTO
            {
                TotalPages = TotalPages,
                OrdersResult = await orders.ToListAsync()
            };

            return Ok(result);
        }

        [Authorize]
        [HttpGet("debug")]
        public IActionResult DebugJwt()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated,
                userName = User.Identity?.Name,
                claims
            });
        }

        // POST api/<OrderController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}
