using Microsoft.AspNetCore.SignalR;
using PetService_Project.Models;
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

        // ✅ 發送訊息，傳送發送者名稱
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            try
            {
                var sender = await _context.TMembers.FindAsync(int.Parse(senderId));
                var senderName = sender?.FName ?? $"ID:{senderId}";

                Console.WriteLine($"📤 {senderName} 傳送訊息給 {receiverId}：{message}");

                if (UserConnections.TryGetValue(receiverId, out var receiverConnId))
                {
                    await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", senderName, message);
                }
                else
                {
                    Console.WriteLine($"⚠️ 找不到接收者 {receiverId} 的連線");
                }

                if (UserConnections.TryGetValue(senderId, out var senderConnId))
                {
                    await Clients.Client(senderConnId).SendAsync("ReceiveMessage", senderName, message);
                }
                else
                {
                    Console.WriteLine($"⚠️ 找不到自己 {senderId} 的連線");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SendMessage 發生錯誤：{ex.Message}");
            }
        }
    }
}
