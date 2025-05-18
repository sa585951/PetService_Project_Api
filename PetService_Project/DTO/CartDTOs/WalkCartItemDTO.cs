namespace PetService_Project_Api.DTO.CartDTOs
{
    public class WalkCartItemDTO
    {
        public int EmployeeServiceId { get; set; }
        public int Amount { get; set; }
        public DateTime WalkStart { get; set; }
        public string Note { get; set; }
    }
}
