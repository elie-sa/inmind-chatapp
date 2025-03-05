using Microsoft.AspNetCore.SignalR;

namespace inmind_chatapp_client;

public class ChatHub: Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}