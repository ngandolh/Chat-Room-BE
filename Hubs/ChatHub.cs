using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat_Room_Demo.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SharedDb _shared;
        public ChatHub(SharedDb shared)
        {
            _shared = shared;
        }

        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All
                .SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined");
        }

        public async Task JoinSpecificChatRoom(UserConnection conn)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);

            _shared.connections[Context.ConnectionId] = conn;

            await Clients.Group(conn.ChatRoom).SendAsync("ReceiveMessage", "admin",
                $"{conn.Username} has joined {conn.ChatRoom}");
        }

        public async Task SendMessage(string msg)
        {
            if (_shared.connections.TryGetValue(Context.ConnectionId, out UserConnection conn))
            {
                await Clients.Groups(conn.ChatRoom).SendAsync("ReceiveSpecificMessage", conn.Username, msg);
            }
        }
    

        public async Task SendMessagePrivate(string msg)
        {
            if (_shared.connections.TryGetValue(Context.ConnectionId, out UserConnection conn))
            {
                // Sử dụng `Clients.Group` với `conn.ChatRoom` để gửi message đến đúng nhóm
                await Clients.Group(conn.ChatRoom).SendAsync("ReceiveMessagePrivate", conn.Username, msg);
            }
        }



        private static readonly Dictionary<Guid, string> SampleUsers = new()
            {
                { Guid.Parse("e2a30c6d-47b3-4b4a-bc95-71fba5b86a1e"), "Customer" },
                { Guid.Parse("b4b6d4fc-95a2-4de9-b3d8-d8b35fb0854f"), "SalesRep" }
            };

        // Get a username by account ID
        private string GetSampleUsername(Guid accountId)
        {
            return SampleUsers.ContainsKey(accountId) ? SampleUsers[accountId] : "Unknown";
        }

        //public async Task JoinPrivateChatRoom(Guid accountId1, Guid accountId2)
        //{
        //    // Generate a unique room ID based on the two account IDs
        //    var roomId = accountId1.CompareTo(accountId2) < 0
        //        ? $"{accountId1}-{accountId2}"
        //        : $"{accountId2}-{accountId1}";

        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        //    var username = GetSampleUsername(accountId1);
        //    _shared.connections[Context.ConnectionId] = new UserConnection
        //    {
        //        Username = username,
        //        ChatRoom = roomId
        //    };

        //    await Clients.Group(roomId).SendAsync("ReceiveJoinNotification", "admin", 
        //        $"{_shared.connections[Context.ConnectionId].Username} has joined the chat in {roomId}");
        //}
        public async Task JoinPrivateChatRoom(Guid accountId1, Guid accountId2)
        {
            // Xác định thứ tự Guid theo một cách cố định, bất kể ai là customer/sales
            //var orderedGuids = new List<Guid> { accountId1, accountId2 };
            //orderedGuids.Sort();

            // Tạo roomId dựa trên thứ tự đã sắp xếp
            //var roomId = $"{orderedGuids[0]}-{orderedGuids[1]}";
            var roomId = accountId2.ToString();

            // Log để kiểm tra roomId cho cả customer và sales
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

    }
}
