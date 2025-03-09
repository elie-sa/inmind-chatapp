using Microsoft.AspNetCore.SignalR.Client;
using Grpc.Net.Client;
using ChatService;

class Program
{
    static async Task Main(string[] args)
    {
        string username;
        do
        {
            Console.Write("Enter your username: ");
            username = Console.ReadLine()?.Trim() ?? "";
        } while (string.IsNullOrEmpty(username));
        
        using var grpcChannel = GrpcChannel.ForAddress("http://localhost:5263");
        var grpcClient = new ChatService.ChatService.ChatServiceClient(grpcChannel);

        // im running signalR and gRPC on different ports since signalR need HTTP 1 while gRPC depends on HTTP 2
        var connection = new HubConnectionBuilder()
            .WithUrl($"http://localhost:5262/chatHub?username={username}")
            .Build();

        connection.On<string, string>("ReceiveMessage", (fromUser, message) =>
        {
            Console.WriteLine($"{fromUser}: {message}");
        });

        connection.On<string[]>("UpdateUserList", (users) =>
        {
            Console.WriteLine("\nOnline Users: " + string.Join(", ", users));
            Console.Write("Enter recipient username: ");
        });

        await connection.StartAsync();
        Console.WriteLine($"Connected as {username}. Type messages below:");

        try
        {
            var chatHistoryResponse = await grpcClient.GetChatHistoryAsync(new ChatHistoryRequest { Username = username });

            if (chatHistoryResponse.Messages.Count == 0)
            {
                Console.WriteLine("No chat history found.");
            }
            else
            {
                Console.WriteLine("\n--- Chat History ---");
                foreach (var message in chatHistoryResponse.Messages)
                {
                    Console.WriteLine($"[{DateTimeOffset.FromUnixTimeMilliseconds(message.Timestamp).UtcDateTime}] {message.FromUser} -> {message.ToUser}: {message.Message}");
                }
                Console.WriteLine("\n--------------------");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching chat history: {ex.Message}");
        }

        while (true)
        {
            Console.Write("Enter recipient username (or type 'list' to see online users): ");
            var toUser = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(toUser)) continue;
            if (toUser.ToLower() == "list")
            {
                var onlineUsers = await connection.InvokeAsync<string[]>("GetOnlineUsers");
                Console.WriteLine("\nOnline Users: " + string.Join(", ", onlineUsers));
                continue;
            }

            Console.Write("Enter message: ");
            var message = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(message)) continue;

            await connection.InvokeAsync("SendMessage", username, toUser, message);
        }
    }
}
