namespace PetService_Project_Api.DTO.FaqDTO
{
    public class FaqListDTO
    {
        public int FId { get; set; }
        public string? FQuestion { get; set; }

        public string? FAnswer { get; set; }

        public string? FCategory { get; set; }

        public DateTime? FCreate_Date { get; set; }
    }
}


