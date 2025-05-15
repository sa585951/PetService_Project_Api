using Microsoft.AspNetCore.SignalR;

namespace PetService_Project_Api.Hubs
{
    public class ChatHub:Hub
    {
        public async Task SendMessage(string user,string message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", user, message);
        }
    }
}
