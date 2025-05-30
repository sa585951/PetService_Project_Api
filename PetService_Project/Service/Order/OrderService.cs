using System.Diagnostics;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO.HotelOrderDTOs;
using PetService_Project_Api.DTO.OrderDTOs;
using PetService_Project_Api.DTO.WalkOrderDTOs;

namespace PetService_Project_Api.Service.Service
{
    public class OrderService : IOrderService
    {
        private readonly dbPetService_ProjectContext _context;
        public OrderService(dbPetService_ProjectContext context)
        {
            _context = context;
        }
        public async Task<OrderPagingResponseDTO> GetOrderAsync(int memberId, OrdersSearchRequestDTO dto)
        { 
            
            //IQuery基本查詢
            var q = _context.TOrders
                .Where(o => o.FMemberId == memberId && !o.FIsDelete)
                .AsQueryable();

            //keyword查編號 or 建立時間
            if(!string.IsNullOrWhiteSpace(dto.keyword))
            {
                var keyword = dto.keyword.Trim();

                //嘗試解析成日期 (支援 yyyy/MM/dd)
                if(DateTime.TryParse(keyword, out var parsedDate))
                {
                    q=q.Where(o=>o.FCreatedAt.HasValue && o.FCreatedAt.Value.Date == parsedDate.Date);
                } else if (int.TryParse(keyword, out var id))
                {
                    q = q.Where(o => o.FId == id);
                }
                
            }

            //ordertype過濾
            if(!string.IsNullOrWhiteSpace(dto.orderType) && dto.orderType != "all")
            {
                if (dto.orderType == "walk")
                    q = q.Where(o => o.FOrderType == "散步");
                if (dto.orderType == "住宿")
                    q = q.Where(o => o.FOrderType == "住宿");
            }

            //過濾付款狀態
            if(!string.IsNullOrWhiteSpace(dto.orderStatus) && dto.orderStatus != "all"){
                q=q.Where(o => o.FOrderStatus == dto.orderStatus);
            };

            //排序
            if (!string.IsNullOrWhiteSpace(dto.sortBy))
            {
                switch (dto.sortBy)
                {
                    case "date_desc":
                        q = q.OrderByDescending(o => o.FCreatedAt);
                        break;
                    case "date_asc":
                        q = q.OrderBy(o => o.FCreatedAt);
                        break;
                    default:
                        q = q.OrderByDescending(o => o.FCreatedAt);
                        break;
                }
            }
            else
            {
                q= q.OrderByDescending (o => o.FCreatedAt);
            }

            //分頁
            int page = dto.page ?? 1;
            int pageSize = dto.pageSize ?? 10;
            int total = await q.CountAsync();
            int totalPages = pageSize > 0
                ? (int)Math.Ceiling(total / (double)pageSize)
                : 1;

            var entities = await q.
                Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtoList = entities.Select(o => new OrderDTO
            {
                Id = o.FId,
                OrderType = o.FOrderType ?? "",
                OrderTypeCode = o.FOrderType == "住宿" ? "hotel" : "walk",
                OrderStatus = o.FOrderStatus,
                TotalAmount = o.FTotalAmount.GetValueOrDefault(0m),
                CreatedAt = o.FCreatedAt.GetValueOrDefault(),
                UpdatedAt = o.FUpdatedAt.GetValueOrDefault()
                
            }).ToList();

            return new OrderPagingResponseDTO
            {
                TotalPages = totalPages,
                OrdersResult = dtoList
            };
        }

        public async Task<int> CreateWalkOrder(int memberId,CreateWalkOrderRequestDTO dto)
        {

            // 檢查資料是否存在
            if (dto.CartItems == null || !dto.CartItems.Any())
                throw new ArgumentException("購物車為空");

            // 取得服務單價 並 計算總金額
            decimal total = 0;
            var detailEntities = new List<TOrderWalkDetail>();

            foreach(var item in dto.CartItems)
            {
                var service = await _context.TEmployeeServices.FindAsync(item.EmployeeServiceId);
                if(service == null)
                    throw new Exception($"遛狗員 {item.EmployeeServiceId} 不存在");

                decimal subtotal = (decimal)(service.FPrice * item.Amount);
                total += subtotal;

                detailEntities.Add(new TOrderWalkDetail
                {
                    FEmployeeServiceId = item.EmployeeServiceId,
                    FWalkStart = item.WalkStart,
                    FWalkEnd = item.WalkStart.AddHours(1),
                    FServicePrice = service.FPrice,
                    FAmount = item.Amount,
                    FTotalPrice = subtotal,
                    FAdditionlＭessage = item.Note
                });
            }
            // 建立訂單
            var order = new TOrder
            {
                FMemberId = memberId,
                FOrderType = "散步",
                FOrderStatus = "未付款",
                FTotalAmount = total,
                FCreatedAt = DateTime.Now,
                FmerchantTradeNo = $"T{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(10, 99)}"
            };

            _context.TOrders.Add(order);
            await _context.SaveChangesAsync(); //先存 才有OrderId

            // 建立訂單明細 指定OrderId
            foreach(var detail in detailEntities)
            {
                detail.FOrderId = order.FId;
                _context.TOrderWalkDetails.Add(detail);
            }

            await _context.SaveChangesAsync();
            // 回傳訂單ID
            return order.FId;
        }

        public async Task<int> CreateHotelOrder(int memberId,CreateHotelOrderRequestDTO dto)
        {
            if (dto.CartItems == null || !dto.CartItems.Any())
                throw new ArgumentException("購物車為空");

            decimal total = 0;
            var detailEntities = new List<TOrderHotelDetail>();

            foreach (var item in dto.CartItems)
            {
                //資料庫讀出房型單日價格
                var roomDetail = await _context.TRoomsDetails.FindAsync(item.RoomDetailId);
                if (roomDetail == null)
                    throw new Exception($"房型明細{item.RoomDetailId}不存在");

                //計算入住天數
                int nights = (item.CheckOut.Date - item.CheckIn.Date).Days;
                if (nights <= 0)
                    throw new Exception("退房日期必須晚於入住日期");

                //取出單日房價、數量
                decimal pricePerRoom = (decimal)roomDetail.FPrice;
                int qty = item.RoomQty;

                //當筆小記 = 天數*單價*數量
                decimal subtotal = nights * pricePerRoom * qty;

                //累加總價
                total += subtotal;

                detailEntities.Add(new TOrderHotelDetail
                {
                    FHotelId = item.HotelId,
                    FRoomDetailId = item.RoomDetailId,
                    FCheckIn = item.CheckIn,
                    FCheckOut = item.CheckOut,
                    FRoomQty = item.RoomQty,
                    FPricePerRoom = pricePerRoom,
                    FTotalPrice = subtotal,
                    FAdditionlMessage = item.AdditionalMessage ?? string.Empty
                });
            }
                
            var order = new TOrder
            {
                    FMemberId = memberId,
                    FOrderType = "住宿",
                    FOrderStatus = "未付款",
                    FTotalAmount = total,
                    FCreatedAt = DateTime.Now,
                    FmerchantTradeNo = $"T{DateTime.Now:yyyyMMddHHmmssfff}{new Random().Next(10, 99)}"
            };
               
            _context.TOrders.Add(order);
            await _context.SaveChangesAsync();
                
            //建立訂單明細 指定OrderId
               
            foreach(var detail in detailEntities)
            {
                detail.FOrderId = order.FId;
                _context.TOrderHotelDetails.Add(detail);
            }
                
            await _context.SaveChangesAsync();
                
            //回傳訂單Id
            return order.FId;
        }

        public async Task SoftDeleteOrderAsync(int memberId, int orderId)
        {
            Debug.WriteLine($"SoftDelete called with memberId = {memberId},orderId = {orderId}");

            var order = await _context.TOrders.FirstOrDefaultAsync(o => 
            o.FId == orderId && 
            o.FMemberId == memberId &&
            !o.FIsDelete);
            if (order == null) throw new KeyNotFoundException("訂單不存在");
            order.FIsDelete = true;
            order.FUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderStatusAsync(int memberId, int orderId, string newStatus)
        {
            var order = await _context.TOrders.FirstOrDefaultAsync(o => o.FId == orderId && o.FMemberId == memberId);
            if (order == null) throw new KeyNotFoundException("訂單不存在");
            order.FOrderStatus = newStatus;
            order.FUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<WalkOrderDetailResponseDTO> GetWalkOrderDetailAsync(int memberId, int orderId)
        {
            var order = await _context.TOrders
                .FirstOrDefaultAsync(o =>
                o.FId == orderId &&
                o.FOrderType == "散步" &&
                o.FMemberId == memberId);

            if (order == null)
                throw new KeyNotFoundException("查無此訂單或無權限查看");

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
                    Note = d.FAdditionlＭessage,
                    EmployeePhoto = d.FEmployeeService.FEmployee.FImage 
                }).ToList()
            };

            return result;
        }
        public async Task<HotelOrderDetailResponseDTO> GetHotelOrderDetailAsync(int memberId, int orderId)
        {
            var order = await _context.TOrders
                .FirstOrDefaultAsync(o =>
                o.FId == orderId &&
                o.FOrderType == "住宿" &&
                o.FMemberId == memberId);

            if (order == null)
                throw new  KeyNotFoundException("查無此訂單或無權限查看");

            var details = await _context.TOrderHotelDetails
                .Where(d => d.FOrderId == orderId)
                .Include(d => d.FHotel)
                .Include(d => d.FRoomDetail)
                .ThenInclude(d=>d.FRoomtype)
                .ToListAsync();

            var result = new HotelOrderDetailResponseDTO
            {
                OrderId = order.FId,
                TotalAmount = (decimal)order.FTotalAmount,
                Status = order.FOrderStatus,
                CreatedAt = (DateTime)order.FCreatedAt.GetValueOrDefault(),
                Items = details.Select(d => new HotelOrderItemResponseDTO
                {
                    HotelName = d.FHotel.FName,
                    RoomName = d.FRoomDetail.FRoomtype.FName,
                    CheckIn = d.FCheckIn.GetValueOrDefault(),
                    CheckOut = d.FCheckOut.GetValueOrDefault(),
                    Qty = d.FRoomQty.GetValueOrDefault(0),
                    PricePerRoom = d.FRoomDetail.FPrice.Value,
                    TotalPrice = d.FTotalPrice.Value,
                    Note = d.FAdditionlMessage,
                    Nights = (d.FCheckOut.Value.Date - d.FCheckIn.Value.Date).Days,
                    HotelPhoto = d.FHotel.FImage1
                }).ToList()
            };

            return result;
        }
    }
}
