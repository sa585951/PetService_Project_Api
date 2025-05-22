using System.ComponentModel.DataAnnotations;

namespace PetService_Project_Api.DTO.OrderDTOs
{
    ///<summary>
    /// 更新訂單狀態用DTO
    /// 呼叫 PATCh /api/Order/{orderId}/status時，放入新的OrderStatus
    /// </summary>
    public class UpdateOrderStatusDTO
    {
        /// <summary>
        /// 新的訂單狀態，例如"未付款"、"已付款"、"已取消"
        /// </summary>
        [Required(ErrorMessage ="OrderStatus 為必填欄位")]
        public string OrderStatus { get; set; }
    }
}
