namespace PetService_Project_Api.DTO
{
    public class ChatSessionDto
    {
        public int FMemberId { get; set; }
        public int FEmployeeId { get; set; }

        public string Role { get; set; } // "member" or "employee"
    }

}
