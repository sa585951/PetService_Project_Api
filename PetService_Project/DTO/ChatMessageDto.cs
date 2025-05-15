namespace PetService_Project_Api.DTO
{
    public class ChatMessageDto
    {
        public int FSessionId { get; set; }
        public int FSenderId { get; set; }
        public string FSenderRole { get; set; }
        public string FMessageText { get; set; }
        public string FAttachmentUrl { get; set; }
        public string FMessageType { get; set; }
    }

}
