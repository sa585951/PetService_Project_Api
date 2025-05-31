using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QaController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public QaController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllQa")]
        public async Task<IActionResult> GetAllQa()
        {
            try
            {
                var result = await _context.TFaqs
                    .GroupBy(q => q.FCategory)
                    .Select(g => new
                    {
                        category = g.Key,
                        qaList = g.Select(q => new
                        {
                            question = q.FQuestion,
                            answer = q.FAnswer
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // 記錄錯誤以利偵錯
                Console.WriteLine($"Error fetching QA data: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
