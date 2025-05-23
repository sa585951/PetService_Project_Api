using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PetService_Project_Api.Models;

namespace PetService_Project_Api.WebSockets
{
    public class WebSocketHandler
    {
        private static readonly List<WebSocket> _sockets = new();

        public static async Task Handle(HttpContext context, WebSocket socket)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                _sockets.Add(socket);
 

                while (socket.State == WebSocketState.Open)
                {
                    await Task.Delay(1000); // 保持連線即可，不接收前端訊息
                }

                _sockets.Remove(socket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
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
