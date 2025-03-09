using ChatService;
using inmind_chatapp_client;
using inmind_chatapp_client.DbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAppDbContext, AppDbContext>();

builder.Services.AddGrpc();
builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();

// adding gRPC
builder.WebHost.ConfigureKestrel(options =>
{
    // gRPC on HTTP 2 (had to configure it on a different port to work)
    options.ListenAnyIP(5263, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    // SignalR on HTTP 1 (port 5262)
    options.ListenAnyIP(5262, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
});


builder.Services.AddGrpc(); // registering grpc
builder.Services.AddSignalR(); // registering signalR


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// needed CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.MapGrpcService<ChatServiceImpl>();

// mapping the signalR hub
app.MapHub<ChatHub>("/chatHub");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();