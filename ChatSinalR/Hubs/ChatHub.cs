using System.Text.Json;
using ChatSinalR.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;

namespace ChatSinalR.Hubs;

public class ChatHub : Hub<IChatClient>
{
    private readonly IDistributedCache _cache;
    public ChatHub(IDistributedCache cache)
    {
        _cache = cache;
    }
    
    public async Task JoinChat(UserConnection connection)
    {
        Console.WriteLine($"Пользователь с именем {connection.UserName} присоединился к чату {connection.ChatRoom}");
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);

        var stringConnection = JsonSerializer.Serialize(connection);
        await _cache.SetStringAsync(Context.ConnectionId, stringConnection);
        
        await Clients
            .Group(connection.ChatRoom)
            .ReceiveMessage("Admin", $"{connection.UserName} Присоединился к чату");
    }

    public async Task SendMessage(string message)
    {
        var stringConnection = await _cache.GetStringAsync(Context.ConnectionId);
        
        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);

        if (connection is not null)
        {
            await Clients
                .Group(connection.ChatRoom)
                .ReceiveMessage(connection.UserName, message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var stringConnection = await _cache.GetStringAsync(Context.ConnectionId);
        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);

        if (connection is not null)
        {
            await _cache.RemoveAsync(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);

            await Clients.Group(connection.ChatRoom)
                .ReceiveMessage("Admin", $"{connection.UserName} покинул чат");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}