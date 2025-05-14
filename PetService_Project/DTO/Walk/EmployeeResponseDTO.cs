namespace PetService_Project_Api.DTO.Walk
{
    public class EmployeeResponseDTO
    {
        public int fEmployeeId { get; set; }           //tEmployee_Service
        public string fName { get; set; }                 // 員工名稱 tEmployee
        public string fDistrictName { get; set; }         //tDistrict  
        public string fDescriptionShort { get; set; }   //tEmployee_Service
        public string fDescription { get; set; }        //tEmployee_Service
        public int fAcceptPetType { get; set; }    // 接受寵物種類 tEmployee_Service
        public int fPetSize { get; set; }            // 寵物體型 tEmployee_Service
        public int fDistance { get; set; }              // 服務距離 tEmployee_Service
        public decimal fPrice { get; set; }             // 單次價格 tEmployee_Service
        public decimal fLatitude { get; set; }          // 緯度 tEmployee_Service
        public decimal fLongitude { get; set; }         // 經度 tEmployee_Service

        public string fImage { get; set; }              // 員工照片 tEmployee
        public string[] fImagepath { get; set; }          // 輪播照片 tEmployee_Photo
    }
}
