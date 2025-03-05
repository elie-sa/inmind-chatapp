using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

static async Task Main(string[] args)
{
    var connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/chatHub")  // Adjust if running on a different port
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();