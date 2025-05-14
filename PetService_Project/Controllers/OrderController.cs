using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.OrderDTOs;
using PetService_Project_Api.Models;


namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]/members")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public OrderController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        // GET: api/Order/members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TOrder>>> GetOrders()
        {
            return await _context.TOrders.ToListAsync();
        }

        // GET api/Order/members/2
        [HttpGet("{memberId}")]
        public async Task<ActionResult> GetOrders(int memberId)
        {
            var orders = await _context.TOrders
                .Where(o => o.FMemberId == memberId)
                .ToListAsync();

            if (orders == null)
                return NotFound();

            return Ok(orders);
        }

        // POST api/<OrderController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}
