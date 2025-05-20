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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "查詢旅館列表失敗"); // 返回錯誤碼和訊息
            }
        }

        //搜尋列搜尋旅館
        [HttpPost("Search")]
        //api/Hotel/Search
        public async Task<IActionResult> SearchHotels([FromBody] HotelSearchDto request)
        {
            try
            {
                double PetCount = request.PetCount / 2.0;
                int requiredRooms = (int)Math.Ceiling(request.PetCount / 2.0);
                if (request.CheckInDate == null || request.CheckOutDate == null)
                {
                    return BadRequest("請提供入住與退房日期");
                }

                int dateRangeCount = (request.CheckOutDate.Value - request.CheckInDate.Value).Days;

                var result = await _context.TQtyStatuses
                    .Where(q => q.FDate >= request.CheckInDate && q.FDate < request.CheckOutDate)
                    .GroupBy(q => q.FHotelId)
                    .Where(g => g.Select(x => x.FDate).Distinct().Count() == dateRangeCount)
                    .Select(g => new
                    {
                        HotelId = g.Key,
                        SmallDogRoom = g.Min(x => x.FSmallDogRoom),
                        MiddleDogRoom = g.Min(x => x.FMiddleDogRoom),
                        BigDogRoom = g.Min(x => x.FBigDogRoom),
                        CatRoom = g.Min(x => x.FCatRoom)
                    })
                    .Where(r =>
                        r.SmallDogRoom >= PetCount ||
                        r.MiddleDogRoom >= PetCount ||
                        r.BigDogRoom >= PetCount ||
                        r.CatRoom >= PetCount
                    )
                    .Select(r => new HotelSearchResponseDto
                    {
                        HotelId = r.HotelId,
                        SmallDogRoom = r.SmallDogRoom,
                        MiddleDogRoom = r.MiddleDogRoom,
                        BigDogRoom = r.BigDogRoom,
                        CatRoom = r.CatRoom,
                        RequiredRooms = requiredRooms // 建議需要的房間數
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "查詢旅館列表失敗");
            }
        }

        [HttpPost("Hoteldetail")]
        //api/Hotel/HotelDetail
        public async Task<IActionResult> SearchHoteldetail([FromBody] HotelSearchDto request)
        {
            try
            {
                if (request.CheckInDate == null || request.CheckOutDate == null)
                {
                    return BadRequest("請提供入住與退房日期");
                }
                int dateRangeCount = (request.CheckOutDate.Value - request.CheckInDate.Value).Days;
                var hotelId = request.HotelId;
                var result = await _context.THotels
                    .Include(h => h.TRoomsDetails)
                    .Include(h => h.THotelItems)
                    .Include(h => h.TRoomsDetails)
                    .ThenInclude(rd => rd.FRoomtype)
                    .Where(h => !h.FIsDelete && h.FId == request.HotelId)
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
                        QtyStatus = h.TQtyStatuses.Where(qty => qty.FDate >= request.CheckInDate && qty.FDate < request.CheckOutDate).Select(qty => new RoomQtyStatus
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
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "查詢旅館列表失敗");
            }
        }
    }
}
