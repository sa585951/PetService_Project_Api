using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelDTO;
using System.Collections.Generic;
using static PetService_Project_Api.DTO.HotelDTO.HotelListPageDTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                        Rating = h.FRating,
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
                if (request.CheckInDate == null || request.CheckOutDate == null)
                {
                    return BadRequest("請提供入住與退房日期");
                }

                var result = await SearchQty(request);

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "查詢旅館列表失敗");
            }
        }

        private async Task<List<HotelSearchResponseDto>> SearchQty(HotelSearchDto request)
        {
            //double PetCount = request.PetCount / 2.0;
            int requiredRooms = request.PetCount;
            int dateRangeCount = (request.CheckOutDate.Value - request.CheckInDate.Value).Days;

            var query = _context.TQtyStatuses
        .Where(q => q.FDate >= request.CheckInDate && q.FDate < request.CheckOutDate);

            // 如果傳入了 HotelId，則加入飯店 ID 的篩選條件
            if (request.HotelId != null)
            {
                query = query.Where(q => q.FHotelId == request.HotelId);
            }

            var result = await query
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
                    r.SmallDogRoom >= request.PetCount ||
                    r.MiddleDogRoom >= request.PetCount ||
                    r.BigDogRoom >= request.PetCount ||
                    r.CatRoom >= request.PetCount
                )
                .Select(r => new HotelSearchResponseDto
                {
                    HotelId = r.HotelId,
                    SmallDogRoom = r.SmallDogRoom,
                    MiddleDogRoom = r.MiddleDogRoom,
                    BigDogRoom = r.BigDogRoom,
                    CatRoom = r.CatRoom,
                    RequiredRooms = requiredRooms
                })
                .ToListAsync();

            return result;
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
                var hotels = await _context.THotels
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
                        Rating = h.FRating,
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
                            Roomtype_id = rd.FRoomtypeId,
                            Price = (int?)rd.FPrice,
                            Image = rd.FImage,
                            Roomsize = rd.FRoom_size
                        }).ToList()
                    }).ToListAsync();
                var HotelDetailQty = await SearchQty(request);
                // 將房型庫存資訊加入對應旅館
                foreach (var hotel in hotels)
                {
                    hotel.QtyStatus = HotelDetailQty
                        .Where(q => q.HotelId == hotel.Id)
                        .Select(q => new RoomQtyStatus
                        {
                            Id = q.Id,
                            HotelId = q.HotelId,
                            SmallDogRoom = q.SmallDogRoom,
                            MiddleDogRoom = q.MiddleDogRoom,
                            BigDogRoom = q.BigDogRoom,
                            CatRoom = q.CatRoom
                        })
                        .ToList();
                }

                //留言
                var Review = await _context.THotelReviews
                    .Where(r => !r.FIsDelete && r.FHotelId == request.HotelId)
                    .Select(r => new ReviewResponseDTO
                    {
                        Id = r.FId,
                        CreatedAt = r.FCreatedAt,
                        Rating = r.FRating,
                        Content = r.FContent,
                        UpdatedAt = r.FUpdatedAt,
                        MemberName = _context.TMembers
                        .Where(m => m.FId == r.FMemberId)
                        .Select(m => m.FName)
                        .FirstOrDefault() ?? "未知使用者",
                        Roomtype = _context.TRoomTypes
                        .Where(rt => rt.FId == r.FRoomtypeId)
                        .Select(rt => rt.FName)
                        .FirstOrDefault() ?? "查無房型",

                    }).ToListAsync();

                var SearchResponse = new HotelListPageDTO
                {
                    Hotels = hotels,
                    HotelDetailQty = HotelDetailQty,
                    Review = Review,
                };
                return Ok(SearchResponse);
            }
            catch (Exception)
            {
                return StatusCode(500, "查詢旅館列表失敗");
            }
        }

        [HttpPost("Create")]
        //api/Hotel/Create
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            try
            {
                var review = new THotelReview
                {
                    FHotelId = dto.HotelId,
                    FRoomtypeId = dto.RoomtypeId,
                    FMemberId = dto.MemberId,
                    FOrderId = dto.OrderId,
                    FRating = dto.Rating,
                    FContent = dto.Content,
                    FCreatedAt = DateTime.Now.AddTicks(-(DateTime.Now.Ticks % TimeSpan.TicksPerSecond)),  //去掉毫秒
                };


                _context.THotelReviews.Add(review);
                await _context.SaveChangesAsync();

                return Ok(review);
            }
            catch (Exception)
            {
                return StatusCode(500, "新增失敗");
            }
        }

        [HttpPut("Edit/{id}")]
        //api/Hotel/Edit/{id}
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            try
            {
                var review = await _context.THotelReviews.FindAsync(id);
                if (review == null || review.FIsDelete)
                {
                    return NotFound(new { message = "找不到評論" });
                }
                review.FRating = dto.Rating;
                review.FContent = dto.Content;
                review.FUpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "修改成功", data = review });
            }
            catch (Exception)
            {
                return StatusCode(500, "修改失敗");
            }
        }

        [HttpPost("Order/{id}")]
        //api/Hotel/Order/{id}
        public async Task<IActionResult> SearchOrder(int id)
        {
            var orderInfo = await _context.TOrders
                .Where(o => o.FId == id)
                .Join(_context.TOrderHotelDetails,
                      o => o.FId,
                      d => d.FOrderId,
                      (o, d) => new HotelReviewOrderInfoDTO
                      {
                          MemberId = o.FMemberId,
                          HotelId = d.FHotelId,
                          RoomtypeId = d.FRoomDetailId
                      })
                .FirstOrDefaultAsync();

            if (orderInfo == null)
            {
                return NotFound("查無訂單資料");
            }

            return Ok(orderInfo);
        }

        [HttpPost("CheckReview")]
        //api/Hotel/CheckReview
        public async Task<IActionResult> CheckOrderReview([FromBody] OrderReviewCheckDTO dto)
        {
            try
            {
                // 找出所有符合會員ID與旅館ID的訂單ID
                var orderIds = await _context.TOrders
                    .Where(o => o.FMemberId == dto.MemberId)
                    .Join(_context.TOrderHotelDetails,
                          o => o.FId,
                          d => d.FOrderId,
                          (o, d) => new { o.FId, d.FHotelId })
                    .Where(x => x.FHotelId == dto.HotelId)
                    .Select(x => x.FId)
                    .ToListAsync();

                if (!orderIds.Any())
                {
                    // 查無任何訂單，視為已全部評論
                    return Ok(new
                    {
                        allReviewed = true,
                        unreviewedOrderIds = new List<int>()
                    });
                }

                // 撈出所有已評論的訂單ID
                var reviewedOrderIds = await _context.THotelReviews
                    .Where(r => r.FOrderId.HasValue)
                    .Select(r => r.FOrderId.Value)
                    .ToListAsync();

                // 找出尚未評論的訂單ID
                var unreviewedOrderIds = orderIds
                    .Where(id => !reviewedOrderIds.Contains(id))
                    .ToList();

                bool allReviewed = !unreviewedOrderIds.Any(); // 若無尚未評論的則為已全部評論

                return Ok(new
                {
                    allReviewed,    //false 有未評論
                    unreviewedOrderIds
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "查詢失敗");
            }
        }


    }
}