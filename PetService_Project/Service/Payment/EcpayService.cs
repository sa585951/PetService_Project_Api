using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PetService_Project_Api.Options;
using PetService_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace PetService_Project_Api.Service.Payment
{
    public class EcpayService : IEcpayService
    {
        private readonly EcpayOptions _options;
        private readonly dbPetService_ProjectContext _context;
        public EcpayService(IOptions<EcpayOptions> options, dbPetService_ProjectContext context)
        {
            _options = options.Value;
            _context = context;
        }

        public string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            //排除 CheckMacValue自己
            var sorted = parameters
                .Where(p => p.Key != "CheckMacValue")
                .OrderBy(p => p.Key)
                .ToDictionary(p => p.Key, p => p.Value);

            //組合字串
            var raw = $"HashKey={_options.HashKey}&" + string.Join("&", sorted.Select(p => $"{p.Key}={p.Value}")) + $"&HashIV={_options.HashIV}";

            // URL Encode 並轉小寫
            var urlEncode = WebUtility.UrlEncode(raw).ToLower();

            // SHA256編碼
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(urlEncode));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }

        public async Task<string> GenerateCheckoutHtmlAsync(string merchantTradeNo, decimal amount, string itemName)
        {
            var dict = await GenerateCheckoutParamsAsync(merchantTradeNo, amount, itemName);

            var sb = new StringBuilder();
            sb.AppendLine($"<form id='ecpayForm' method='POST' action='{_options.GatewayUrl}'>");
            foreach (var kv in dict)
            {
                sb.AppendLine($"<input type='hidden' name='{kv.Key}' value='{kv.Value}' />");
            }
            sb.AppendLine("</form>");
            sb.AppendLine("<script>document.getElementById('ecpayForm').submit();</script>");
            return sb.ToString();
        }

        public async Task<Dictionary<string, string>> GenerateCheckoutParamsAsync(string merchantTradeNo, decimal amount, string itemName)
        {
            var dict = new Dictionary<string, string>
            {
                {"MerchantID", _options.MerchantId },
                {"MerchantTradeNo", merchantTradeNo },
                {"MerchantTradeDate",DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                {"PaymentType","aio" },
                {"TotalAmount",((int)amount).ToString() },
                {"TradeDesc","訂單付款" },
                {"ItemName",itemName },
                {"ReturnURL",_options.ReturnUrl},
                {"ClientBackURL",_options.ClientBackUrl},
                {"ChoosePayment","Credit" },
                {"EncryptType","1" }
            };

            //加入CheckMacValue
            dict["CheckMacValue"] = GenerateCheckMacValue(dict);
            return dict;
        }

        public async Task<bool> ProcessCallbackAsync(IFormCollection form)
        {
            var rtnCode = form["RtnCode"].ToString(); //1代表成功
            var merchantTradeNo = form["MerchantTradeNo"].ToString();

            if (rtnCode == "1")
            {
                var order = await _context.TOrders.FirstOrDefaultAsync(o => o.FmerchantTradeNo == merchantTradeNo);
                if (order != null)
                {
                    order.FOrderStatus = "已付款";
                    order.FpaymentTime = DateTime.Now;
                    order.FUpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }
    }
}
