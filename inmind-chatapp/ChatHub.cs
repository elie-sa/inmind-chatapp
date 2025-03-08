using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace inmind_chatapp_client;

public class ChatHub : Hub
{
    // used a concurrent dictionary because it is thread-safe
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

    public override async Task OnConnectedAsync()
    {
        var username = Context.GetHttpContext()?.Request.Query["username"];
        if (!string.IsNullOrEmpty(username))
        {
            ConnectedUsers[username] = Context.ConnectionId;
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Keys);
            await Clients.All.SendAsync("ReceiveMessage", "System", $"{username} has joined the chat.");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var user = ConnectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
        if (user != null)
        {
            ConnectedUsers.TryRemove(user, out _);
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Keys);
            await Clients.All.SendAsync("ReceiveMessage", "System", $"{user} has left the chat.");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string[]> GetOnlineUsers()
    {
        return ConnectedUsers.Keys.ToArray();
    }

    public async Task SendMessage(string fromUser, string toUser, string message)
    {
        if (ConnectedUsers.TryGetValue(toUser, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromUser, message);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"{toUser} is offline or unavailable.");
        }
    }
}