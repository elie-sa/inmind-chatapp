using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

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