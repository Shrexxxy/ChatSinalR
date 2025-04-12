using ChatSinalR.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatSinalR.Hubs;

public class ChatHub : Hub<IChatClient>
{
    public async Task JoinChat(UserConnection connection)
    {
        Console.WriteLine($"Пользователь с именем {connection.UserName} присоединился к чату {connection.ChatRoom}");
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);

        await Clients
            .Group(connection.ChatRoom)
            .ReceiveMessage("Admin", $"{connection.UserName} Присоединился к чату");
    }
    
}