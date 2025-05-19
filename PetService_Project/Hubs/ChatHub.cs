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

        //    public async Task SendMessage(string senderId, string receiverId, string message)
        //    {
        //        // ✅ 嘗試找雙方的會話
        //        if (!int.TryParse(senderId, out var parsedSenderId) ||
        //            !int.TryParse(receiverId, out var parsedReceiverId))
        //        {
        //            Console.WriteLine("❌ senderId 或 receiverId 轉型失敗");
        //            return;
        //        }
        //        var sender = await _context.TMembers.FindAsync(int.Parse(senderId));
        //        var senderName = sender?.FName ?? $"ID:{senderId}";

        //        var session = await _context.TChatSessions
        //            .FirstOrDefaultAsync(s =>
        //                (s.FMemberId == parsedSenderId && s.FEmployeeId == parsedSenderId ||
        //                 s.FMemberId == parsedReceiverId && s.FEmployeeId == parsedReceiverId) &&
        //                s.Status == "0");

        //        if (session == null)
        //        {
        //            Console.WriteLine("⚠ 無有效會話，訊息不儲存");
        //        }
        //        else
        //        {
        //            // ✅ 寫入資料庫
        //            var newMsg = new TChatMessage
        //            {
        //                FSessionId = session.FSessionId,
        //                FSenderId = parsedSenderId,
        //                FSenderRole = "unknown", // 👉 你可以改成 member/employee 判斷
        //                FMessageText = message,
        //                FSendTime = DateTime.Now,
        //                FMessageType = "text",
        //                FIsDeleted = false
        //            };

        //            _context.TChatMessages.Add(newMsg);
        //            await _context.SaveChangesAsync();

        //            Console.WriteLine($"💾 已儲存訊息：{message}，會話ID：{session.FSessionId}");
        //        }

        //        // ✅ 發送訊息給對方
        //        if (UserConnections.TryGetValue(receiverId, out var receiverConnId))
        //        {
        //            await Clients.Client(receiverConnId).SendAsync("ReceiveMessage", senderName, message);
        //        }

        //        // ✅ 自己也顯示
        //        await Clients.Caller.SendAsync("ReceiveMessage", senderName, message);
        //    }
        //}

        //public async Task SendMessage(ChatMessageDto dto)
        //{
        //    try
        //    {
        //        var otherConnectionId = UserConnections
        //            .Where(x => x.Key != dto.FSenderId.ToString())
        //            .Select(x => x.Value)
        //            .FirstOrDefault();

        //        if (!string.IsNullOrEmpty(otherConnectionId))
        //        {
        //            await Clients.Client(otherConnectionId)
        //                .SendAsync("ReceiveMessage", dto.FSenderId.ToString(), dto.FMessageText);
        //        }

        //        await Clients.Caller.SendAsync("ReceiveMessage", dto.FSenderId.ToString(), dto.FMessageText);

        //        var chatMsg = new TChatMessage
        //        {
        //            FSessionId = dto.FSessionId,
        //            FSenderId = dto.FSenderId,
        //            FSenderRole = dto.FSenderRole,
        //            FMessageText = dto.FMessageText,
        //            FAttachmentUrl = dto.FAttachmentUrl ?? "",
        //            FMessageType = dto.FMessageType ?? "text",
        //            FSendTime = DateTime.Now,
        //            FIsRead = false,
        //            FIsDeleted = false
        //        };

        //        _context.TChatMessages.Add(chatMsg);
        //        await _context.SaveChangesAsync();

        //        Console.WriteLine($"✅ 已儲存訊息：{dto.FMessageText}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ SendMessage 失敗：{ex.Message}");
        //        throw;
        //    }
        //}

        //✅ 發送訊息，傳送發送者名稱
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

