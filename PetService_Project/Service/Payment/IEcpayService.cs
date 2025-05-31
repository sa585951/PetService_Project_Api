namespace PetService_Project_Api.Service.Payment
{
    public interface IEcpayService
    {
        
        ///<summary>產生送到前端的參數(JSON)，讓前端自行組表單/開新視窗跳轉綠界</summary>
        Task<Dictionary<string, string>> GenerateCheckoutParamsAsync(string merchantTradeNo, decimal amount, string itemName);
        /// <summary>產生一段自動submit 的HTML form(直接在後端回HTML)</summary>
        Task<string>GenerateCheckoutHtmlAsync(string merchantTradeNo,decimal amount, string itemName);
        ///<summary> 綠界 callback，處理付款結果並更新訂單狀態</summary>
        Task<bool> ProcessCallbackAsync(IFormCollection form);
        ///<summary>產生氯界需要的CheckMacValue</summary>
        string GenerateCheckMacValue(Dictionary<string, string> parameters);
    }
}
