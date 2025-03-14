using System.Collections.Concurrent;
using inmind_chatapp_client.DbContext;
using inmind_chatapp_client.Models;
using Microsoft.AspNetCore.SignalR;

namespace inmind_chatapp_client;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
    private IAppDbContext _context;

    public ChatHub(IAppDbContext context)
    {
        _context = context;
    }
    
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
            var chatMessage = new Message
            {
                FromUser = fromUser,
                ToUser = toUser,
                MessageText = message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromUser, message);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"{toUser} is offline or unavailable.");
        }
    }
}