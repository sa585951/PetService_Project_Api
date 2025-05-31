namespace PetService_Project_Api.DTO.NewsDTO
{
    public class NewsListDTO
    {
        public int FId { get; set; }
        public string? FTitle { get; set; }

        public string? FContent { get; set; }

        public string? FCategory { get; set; }

        public DateTime? FCreate_Date { get; set; }
    }
}