using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Entity;
using Chat_Room_Demo.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Chat_Room_Demo.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SharedDb _shared;
        private readonly ILogger _logger;

        public ChatHub(SharedDb shared, ILogger<ChatHub> logger)
        {
            _shared = shared;
            _logger = logger;
        }

        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined");
        }

        public async Task JoinSpecificChatRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);

            _shared.connections[Context.ConnectionId] = conn;

            await Clients.Group(conn.ChatRoom).SendAsync("ReceiveMessage", "admin",
                $"{conn.Username} has joined {conn.ChatRoom}");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogError(exception, $"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("ReceiveJoinNotification", Context.ConnectionId, "has joined the room.");
        }

        public async Task SendMessage(string roomId, string user, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message);
        }

        //public async Task SendMessagePrivate(string msg)
        //{
        //    if (_shared.connections.TryGetValue(Context.ConnectionId, out UserConnection conn))
        //    {
        //        await Clients.Group(conn.ChatRoom).SendAsync("ReceiveMessagePrivate", conn.Username, msg);
        //    }
        //}

        public async Task JoinPrivateChatRoom(Guid accountId1, Guid accountId2)
        {
            var roomId = accountId2.ToString();
            Console.WriteLine($"User {accountId1} or {accountId2} joining room {roomId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var username = GetSampleUsername(accountId1);
            _shared.connections[Context.ConnectionId] = new UserConnection
            {
                Username = username,
                ChatRoom = roomId
            };

            await Clients.Group(roomId).SendAsync("ReceiveJoinNotification", "admin",
                $"{_shared.connections[Context.ConnectionId].Username} has joined the chat in {roomId}.");
        }

        public async Task JoinPrivateRoom(Guid accountId1, Guid accountId2)
        {
            var connectionId = Context.ConnectionId;
            await Clients.Caller.SendAsync("    Result", await JoinRoomPrivate(accountId1, accountId2, connectionId));
        }

        private async Task<Guid> JoinRoomPrivate(Guid accountId1, Guid accountId2, string connectionId)
        {
            // Your logic or call ChatService if needed (use IHubContext if ChatService calls ChatHub)
            // Temporary stub return
            return Guid.NewGuid();
        }

        private string GetSampleUsername(Guid accountId)
        {
            var SampleUsers = new Dictionary<Guid, string>
            {
                { Guid.Parse("e2a30c6d-47b3-4b4a-bc95-71fba5b86a1e"), "Customer" },
                { Guid.Parse("b4b6d4fc-95a2-4de9-b3d8-d8b35fb0854f"), "SalesRep" }
            };
            return SampleUsers.ContainsKey(accountId) ? SampleUsers[accountId] : "Unknown";
        }
    }
}
