using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5262/chatHub")  // Adjust if running on a different port
            .Build();

        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.WriteLine($"{user}: {message}");
        });

        await connection.StartAsync();
        Console.WriteLine("Connected to chat. Type messages below:");

        while (true)
        {
            string message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) continue;

            await connection.InvokeAsync("SendMessage", "User1", message);
        }
    }
}