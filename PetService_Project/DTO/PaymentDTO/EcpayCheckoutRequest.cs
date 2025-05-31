namespace PetService_Project_Api.DTO.PaymentDTO
{
    public class EcpayCheckoutRequest
    {
        public int OrderId { get; set; }
        public int TotalAmount { get; set; }
        public string ItemName { get; set; }
    }
}
