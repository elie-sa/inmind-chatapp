using Grpc.Core;
using inmind_chatapp_client.DbContext;
using Microsoft.EntityFrameworkCore;

namespace ChatService;

public class ChatServiceImpl : ChatService.ChatServiceBase
{
    private readonly IAppDbContext _context;

    public ChatServiceImpl(IAppDbContext context)
    {
        _context = context;
    }

    public override async Task<ChatHistoryResponse> GetChatHistory(ChatHistoryRequest request, ServerCallContext context)
    {
        var messages = await _context.Messages
            .Where(m => m.FromUser == request.Username || m.ToUser == request.Username)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        var response = new ChatHistoryResponse();
        response.Messages.AddRange(messages.Select(m => new ChatMessage
        {
            FromUser = m.FromUser,
            ToUser = m.ToUser,
            Message = m.MessageText,
            Timestamp = m.Timestamp
        }));

        return response;
    }
}