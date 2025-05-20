namespace PetService_Project_Api.DTO.MemberDTO
{
    public class AccountEmailSignupRequestDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public List<int> Sources { get; set; }
    }
}
