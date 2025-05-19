using PetService_Project.Models;
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
        public async Task<int> CreateWalkOrder(CreateWalkOrderRequestDTO dto, string memberId)
        {
            //memberId型別轉換
            if(!int.TryParse(memberId, out int memberIdInt))
                throw new Exception("無法解析會員 ID，請確認登入狀態或 Token");

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
                FMemberId = memberIdInt,
                FOrderType = "散步",
                FOrderStatus = "未付款",
                FTotalAmount = total,
                FCreatedAt = DateTime.Now
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
    }
}
