namespace PetService_Project_Api.DTO
{
    public class MarkAsReadDto
    {
        public int SessionId { get; set; }     // 對應 fSession_id
        public int UserId { get; set; }     // 對應 fSender_id 的「接收方」
    }
}
