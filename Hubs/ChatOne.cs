using Microsoft.AspNetCore.SignalR;

namespace Chat_Room_Demo.Hubs
{
    public class ChatOne : Hub
    {
        private static Dictionary<string, List<string>> roomUsers = new Dictionary<string, List<string>>();

        public async Task JoinRoom(string roomId, string username)
        {
            // Kiểm tra nếu room đã tồn tại và có nhiều nhất 2 người
            if (roomUsers.ContainsKey(roomId))
            {
                if (roomUsers[roomId].Count >= 2)
                {
                    await Clients.Caller.SendAsync("RoomFull", "This room is full.");
                    return;
                }
                roomUsers[roomId].Add(Context.ConnectionId);
            }
            else
            {
                roomUsers[roomId] = new List<string> { Context.ConnectionId };
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoined", username);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var room in roomUsers)
            {
                if (room.Value.Contains(Context.ConnectionId))
                {
                    room.Value.Remove(Context.ConnectionId);
                    await Clients.Group(room.Key).SendAsync("UserLeft", Context.ConnectionId);
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToRoom(string roomId, string user, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message);
        }
    }
}
