using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelDTO;

namespace PetService_Project_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly dbPetService_ProjectContext _context;

        public HotelController(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        //Hotel
        [HttpGet]
        public async Task<IActionResult> GetAllHotels()
        {
            var hotels = await _context.THotels
                .Where(h => h.FIsDelete == false)
                .ToListAsync();

            return Ok(hotels);
        }

        // 搜尋旅館
        //[HttpPost("search")]
        //public async Task<IActionResult> SearchHotels([FromBody] HotelSearchDto dto)
        //{
        //    var query = _context.THotels
        //        .Include(h => h.TRoomsDetail)
        //        .Include(h => h.THotelItems)
        //        .Where(h => h.FIsDelete == false)
        //        .AsQueryable();

        //    // 篩選服務
        //    if (dto.Service != null && dto.Service.Any())
        //    {
        //        query = query.Where(h => h.THotelItems.Any(i => i.FType == 1 && dto.Service.Contains(i.FName)));
        //    }

        //    // 篩選設施
        //    if (dto.Amenity != null && dto.Amenity.Any())
        //    {
        //        query = query.Where(h => h.THotelItems.Any(i => i.FType == 0 && dto.Amenity.Contains(i.FName)));
        //    }

        //    // 其他條件（例如入住日期、房型數量等）可再根據需求進一步加

        //    var result = await query.ToListAsync();

        //    return Ok(result);
        //}

        // GET: HotelController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HotelController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HotelController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HotelController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
