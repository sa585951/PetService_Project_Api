namespace PetService_Project_Api.DTO.WalkDTOs
{
    public class EmployeeListRequestDTO
    {
        public string? District { get; set; }        
        public List<string>? PetTypes { get; set; }        
        public List<string>? PetSize { get; set; }        
        public int? Price { get; set; }   
    }
}
