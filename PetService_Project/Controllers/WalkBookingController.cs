using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalkBookingController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public WalkBookingController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        // GET: api/WalkBooking
        [HttpGet]
        public async Task<IActionResult> GetEmployeeServices()
        {
            var employeeServices = await _context.TEmployeeServices
                .Where(e => e.FIsDelete == false)
                .ToListAsync();

            return Ok(employeeServices);
        }
    }
}
