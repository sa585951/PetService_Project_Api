using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetService_Project.Models;
using PetService_Project_Api.DTO;
using System.Collections.Concurrent;

namespace PetService_Project_Api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly dbPetService_ProjectContext _context;

        // constructor 注入資料庫 context
        public ChatHub(dbPetService_ProjectContext context)
        {
            _context = context;
        }

        // 記錄 userId 對應的 connectionId
        public static ConcurrentDictionary<string, string> UserConnections = new();

        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
                Console.WriteLine($"✅ 使用者 {userId} 已連線，連線 ID：{Context.ConnectionId}");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.TryRemove(userId, out _);
                Console.WriteLine($"❌ 使用者 {userId} 已離線");
            }
            return base.OnDisconnectedAsync(exception);
        }

        //public async Task SendMessage(string senderId, string receiverId, string message)
        //{
        //    Console.WriteLine($"🔍 Debug 傳入 senderId = {senderId}, receiverId = {receiverId}, message = {message}");
        //    Console.WriteLine($"➡️ SendMessage 被呼叫：senderId={senderId}, receiverId={receiverId}, message={message}");
        //    try
        //    {
        //        var sender = await _context.TMembers.FindAsync(int.Parse(senderId));
        //        var senderName = sender?.FName ?? $"ID:{senderId}";

        //        Console.WriteLine($"📤 {senderName} 傳送訊息給 {receiverId}：{message}");

        //        if (UserConnections.TryGetValue(receiverId, out var receiverConnId))
        //        {
        //            await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", senderName, message);
        //        }
        //        else
        //        {
        //            Console.WriteLine($"⚠️ 找不到接收者 {receiverId} 的連線");
        //        }

        //        if (UserConnections.TryGetValue(senderId, out var senderConnId))
        //        {
        //            await Clients.Client(senderConnId).SendAsync("ReceiveMessage", senderName, message);
        //        }
        //        else
        //        {
        //            Console.WriteLine($"⚠️ 找不到自己 {senderId} 的連線");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ SendMessage 發生錯誤：{ex}");
        //        throw; // 💥 不要吞掉例外，讓前端知道錯在哪裡
        //    }
        //}
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            Console.WriteLine($"🔍 傳入 senderId={senderId}, receiverId={receiverId}, message={message}");

            try
            {
                int senderIntId = int.Parse(senderId);

                var sender = await _context.TMembers.FindAsync(senderIntId);

                string senderName = sender?.FName ?? $"ID:{senderId}";
                string senderAvatar = string.IsNullOrWhiteSpace(sender?.FImage)
                    ? "/uploads/avatars/default-avatar.jpg"
                    : sender.FImage;

                var payload = new
                {
                    senderId = senderId,
                    senderName = senderName,
                    senderAvatar = senderAvatar,
                    messageText = message
                };

                // 🔄 發送給接收者
                if (UserConnections.TryGetValue(receiverId, out var receiverConnId))
                {
                    await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", payload);
                }

                // 🔄 回送給自己
                if (UserConnections.TryGetValue(senderId, out var senderConnId))
                {
                    await Clients.Client(senderConnId).SendAsync("ReceiveMessage", payload);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SendMessage 發生錯誤：{ex}");
                throw;
            }
        }
    }
}

