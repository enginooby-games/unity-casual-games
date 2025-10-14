using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleSignalRGame.Server;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR service
builder.Services.AddSignalR();

// Add CORS for Unity client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://127.0.0.1:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

// Map SignalR hub endpoint
app.MapHub<GameHub>("/gamehub");

// Simple root endpoint for testing
app.MapGet("/", () => "SignalR Game Server is running! 🎮");

Console.WriteLine("========================================");
Console.WriteLine("SignalR Server running on:");
Console.WriteLine("  http://localhost:5000");
Console.WriteLine("  Hub: http://localhost:5000/gamehub");
Console.WriteLine("========================================");

app.Run();