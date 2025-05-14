namespace PetService_Project_Api.DTO.Walk
{
    public class EmployeeSelectionRequestDTO
    {
        public int fEmployeeId { get; set; }         // tEmployee_Service
        public int fAcceptPetType { get; set; }      // 寵物類型tEmployee_Service
        public int fPetSize { get; set; }            // 寵物體型tEmployee_Service
        public int fAmount { get; set; }                // 寵物數量tOrder_Walk_Detail
        public DateOnly fBookingDate { get; set; }        // 與 fBookingTime 組合後，對應 Order_Walk_Detail.fWalkStart
        public TimeOnly fBookingTime { get; set; }        // 與 fBookingDate 組合後，對應 Order_Walk_Detail.fWalkStart
        public string fAdditionlMessage { get; set; }   // 備註tOrder_Walk_Detail
        public decimal fServicePrice { get; set; }      // 單價 tOrder_Walk_Detail
        public decimal fTotalPrice { get; set; }     // 總價 tOrder_Walk_Detail

    }
}
