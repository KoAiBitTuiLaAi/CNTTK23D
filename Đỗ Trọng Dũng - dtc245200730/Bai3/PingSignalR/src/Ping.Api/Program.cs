// PingSignalR – Ping.Api (Bài 3: máy chủ SignalR)
// Chạy: dotnet run --project src/Ping.Api --urls http://localhost:5200
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Việc 1: đăng ký dịch vụ SignalR vào DI (phải đặt TRƯỚC builder.Build())
builder.Services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "PingApi", status = "running" }));

// Việc 2: ánh xạ hub vào tuyến /hubs/ping
app.MapHub<PingHub>("/hubs/ping");

app.Run();

sealed class PingHub : Hub
{
    // Máy khách gọi Echo -> trả lời RIÊNG người gọi
    public async Task Echo(string message)
    {
        await Clients.Caller.SendAsync("EchoResponse", $"Echo: {message}");
    }
}
