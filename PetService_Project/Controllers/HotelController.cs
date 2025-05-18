using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelDTO;
using System.Collections.Generic;
using static PetService_Project_Api.DTO.HotelDTO.HotelListPageDTO;

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

        //api/Hotel
        [HttpGet]
        public async Task<IActionResult> GetAllHotels()
        {
            //var hotels = await _context.THotels
            //    .Where(h => h.FIsDelete == false)
            HotelListPageDTO result = null;
            try
            {
                var today = DateTime.Now.Date;
                var hotels = await _context.THotels
                    .Include(h => h.TRoomsDetails)
                    .Include(h => h.THotelItems)
                    .Include(h => h.TRoomsDetails)
                    .ThenInclude(rd => rd.FRoomtype)
                    .Where(h => !h.FIsDelete)
                    .Select(h => new HotelListDto
                    {
                        Id = h.FId,
                        Name = h.FName,
                        Phone = h.FPhone,
                        Address = h.FAddress,
                        Email = h.FEmail,
                        Longitude = h.FLongitude,
                        Latitude = h.FLatitude,
                        Image_1 = h.FImage1,
                        Image_2 = h.FImage2,
                        Image_3 = h.FImage3,
                        RoomTypes = h.TRoomsDetails.Select(rt => new RoomTypeDto
                        {
                            Id = rt.FId,
                            Name = rt.FRoomtype.FName
                        }).ToList(),
                        Items = h.THotelItems.Select(i => new HotelItemDto
                        {
                            Id = i.FId,
                            Name = i.FName,
                        }).ToList(),
                        RoomDetail = h.TRoomsDetails.Select(rd => new RoomDetailDto
                        {
                            Id = rd.FId,
                            Price = (int?)rd.FPrice,
                            Image = rd.FImage,
                            Roomsize = rd.FRoom_size
                        }).ToList(),
                        QtyStatus = h.TQtyStatuses.Where(qty => qty.FDate == today).Select(qty => new RoomQtyStatus
                        {
                            Id = qty.FId,
                            SmallDogRoom = qty.FSmallDogRoom,
                            MiddleDogRoom = qty.FMiddleDogRoom,
                            BigDogRoom = qty.FBigDogRoom,
                            CatRoom = qty.FCatRoom,
                        }).ToList(),
                        Review = h.THotelReviews.Select(hr => new HotelReview
                        {
                            Id = hr.FId,
                            CreatedAt = hr.FCreatedAt,
                            Rating = hr.FRating,
                            Content = hr.FContent,
                            UpdatedAt = hr.FUpdatedAt,
                        }).ToList()
                    }).ToListAsync();

                // 查詢不重複的設施與服務
                var totalItems = await _context.THotelItems
                    .Where(i => i.FName != null)
                    .GroupBy(i => i.FName)
                    .Select(g => g.Select(i => new HotelItemDto
                    {
                        Id = i.FId,
                        Name = i.FName,
                        Description = i.FDescription
                    }).First())
                    .ToListAsync();


                // 包裝回傳結果
                result = new HotelListPageDTO
                {
                    Hotels = hotels,
                    TotalItems = totalItems
                };

            }
            catch (Exception ex) {
                return StatusCode(500, "查詢旅館列表失敗"); // 返回錯誤碼和訊息
            }
            return Ok(result);
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
        
        
    }
}
