using Chat_Room_Demo.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat_Room_Demo.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All
                .SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined");
        }

        public async Task JoinSpecificChatRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);
            await Clients.Group(conn.ChatRoom).SendAsync("ReceiveMessage", "admin", 
                $"{conn.Username} has joined {conn.ChatRoom}");
        }
    }
}
