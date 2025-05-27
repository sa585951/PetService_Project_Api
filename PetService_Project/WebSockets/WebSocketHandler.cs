using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetService_Project.Models;
using PetService_Project_Api.Models;

namespace PetService_Project_Api.WebSockets
{

    public class WebSocketHandler
    {
        private static async Task<bool> IsAdminAsync(string token, IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, parameters, out _);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return false;

                var user = await userManager.FindByIdAsync(userId);
                if (user == null) return false;

                var roles = await userManager.GetRolesAsync(user);
                return roles.Contains("Admin");
            }
            catch
            {
                return false;
            }
        }


        private static readonly List<WebSocket> _sockets = new();

        public static async Task Handle(HttpContext context, IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var token = context.Request.Query["token"].ToString();

            using var scope = scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (!await IsAdminAsync(token, config, userManager))
            {
                context.Response.StatusCode = 403;
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            _sockets.Add(socket);

            var db = scope.ServiceProvider.GetRequiredService<dbPetService_ProjectContext>();

            // 1. 查詢每日註冊數
            var dateStats = await db.TMembers
                .Where(m => m.FCreatedDate != null && !m.FIsDeleted)
                .GroupBy(m => m.FCreatedDate.Value.Date)
                .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                .ToDictionaryAsync(g => g.Date, g => g.Count);

            var dateMessage = JsonSerializer.Serialize(new
            {
                type = "daily_registrations",
                data = dateStats
            });
            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(dateMessage)), WebSocketMessageType.Text, true, CancellationToken.None);

            // 2. 查詢來源百分比
            // 2-1. 取得所有被記錄的來源管道的總次數，作為計算百分比的基數
            var totalSources = await db.TMemberSources.CountAsync();

            // 2-2. 查詢每個來源管道被選擇的次數，並聯結來源名稱
            var sourceCounts = await db.TMemberSources
                .GroupBy(x => x.FSourceId)
                .Select(g => new { fSourceId = g.Key, source_count = g.Count() }) // 計算每個來源 ID 出現的次數
                .OrderByDescending(x => x.source_count)
                .Join(
                    db.TSourceLists,
                    sourceCount => sourceCount.fSourceId,
                    sourceList => sourceList.FSourceId,
                    (sourceCount, sourceList) => new
                    {
                        fSourceId = sourceCount.fSourceId,
                        sourceName = sourceList.FSourceName, // 假設 tSourceList 中來源名稱的欄位是 FName
                        source_count = sourceCount.source_count
                    })
                .ToListAsync();

            // 2-3. 計算每個來源管道的百分比
            var statsWithPercentage = sourceCounts.Select(s => new
            {
                sourceName = s.sourceName,
                percentage = totalSources > 0 ? (double)s.source_count / totalSources * 100 : 0
            }).OrderByDescending(s => s.percentage).ToList();

            var sourceMessage = JsonSerializer.Serialize(new
            {
                type = "member_source_percentages",
                data = statsWithPercentage
            });
            Console.WriteLine(sourceMessage);
            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(sourceMessage)), WebSocketMessageType.Text, true, CancellationToken.None);

            // 保持連線
            while (socket.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }

            _sockets.Remove(socket);
        }


        public static async Task BroadcastAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var socket in _sockets.ToList())
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
