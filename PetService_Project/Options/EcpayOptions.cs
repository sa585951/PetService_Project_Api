namespace PetService_Project_Api.Options
{
    public class EcpayOptions
    {
        public string MerchantId { get; set; }
        public string HashKey { get; set; }
        public string HashIV { get; set; }
        public string GatewayUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string ClientBackUrl { get; set; }
    }
}
