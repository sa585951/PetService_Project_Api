using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public NewsController(dbPetService_ProjectContext context)
        {
            _context = context;
        }
        [HttpGet("GetNewsTitle")]
        public async Task<IActionResult> GetNewsTitle()
        {
            try
            {
                var result = await _context.TNews
                    .GroupBy(q => q.FCategory)
                    .Select(g => new
                    {
                        category = g.Key,
                        newsList = g.Select(q => new
                        {
                            id = q.FId,
                            title = q.FTitle,
                            content = q.FContent,
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                // 記錄錯誤以利偵錯
                Console.WriteLine($"Error fetching news titles: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var news = await _context.TNews
                    .Where(n => n.FId == id)
                    .Select(n => new
                    {
                        id = n.FId,
                        title = n.FTitle,
                        content = n.FContent
                    })
                    .FirstOrDefaultAsync();

                if (news == null)
                    return NotFound("找不到該公告");

                return Ok(news);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching news by ID: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}