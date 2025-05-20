namespace PetService_Project_Api.DTO.WalkDTOs
{
    public class EmployeeListResponseDTO
    {
        public int EmployeeId { get; set; }      
        public string Name { get; set; }          
        public string District { get; set; }      
        public List<string> PetTypes { get; set; }       
        //public string PetSize { get; set; }       
        public int Price { get; set; }            
        public string EmployeeImage { get; set; }     
        
    }
}
