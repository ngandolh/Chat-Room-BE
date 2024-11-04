using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Entity;
using Chat_Room_Demo.Models;
using Domain.Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat_Room_Demo.Hubs
{
    public class ChatHub : Hub
    {
        private readonly SharedDb _shared;
        private readonly ChatRoomContext _context;
        public ChatHub(SharedDb shared, ChatRoomContext context)
        {
            _shared = shared;
            _context = context;
        }

        public async Task SendMessage(ChatMessage chatMessage)
        {
            await Clients.All.SendAsync("Send", chatMessage);
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

        //public async Task SendMessage(string msg)
        //{
        //    if (_shared.connections.TryGetValue(Context.ConnectionId, out UserConnection conn))
        //    {
        //        await Clients.Groups(conn.ChatRoom).SendAsync("ReceiveSpecificMessage", conn.Username, msg);
        //    }
        //}
    

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

        public async Task JoinPrivateChatRoom(Guid accountId1, Guid accountId2)
        {
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

        public async Task JoinRoomPrive(Guid accountId1, Guid accountId2)
        {
            //Customer gửi 2 CustomerId - SaleId
            //Check room (table) đã tồn tại room chưa
            //Case 1: Chưa có room -> Tạo room
            var isRoom = await _context.Rooms.FirstOrDefaultAsync(r =>
                       (r.CustomerId == accountId1 && r.SaleId == accountId2) ||
                       (r.CustomerId == accountId2 && r.SaleId == accountId1));
            if (isRoom == null)
            {
                var newRoom = new Room
                {
                    Id = Guid.NewGuid(),
                    CustomerId = accountId1,
                    SaleId = accountId2,
                    InsDate = DateTime.Now
                };

                await _context.Rooms.AddAsync(newRoom);
                await _context.SaveChangesAsync();

                await Groups.AddToGroupAsync(Context.ConnectionId, newRoom.Id.ToString());

                var username = GetSampleUsername(accountId1);
                _shared.connections[Context.ConnectionId] = new UserConnection
                {
                    Username = username,
                    ChatRoom = newRoom.Id.ToString(),
                };

                await Clients.Group(newRoom.Id.ToString()).SendAsync("ReceiveJoinNotification", "admin",
                    $"{_shared.connections[Context.ConnectionId].Username} has joined the chat in {newRoom.Id.ToString()}.");
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, isRoom.Id.ToString());

                var username = GetSampleUsername(accountId1);
                _shared.connections[Context.ConnectionId] = new UserConnection
                {
                    Username = username,
                    ChatRoom = isRoom.Id.ToString(),
                };

                await Clients.Group(isRoom.Id.ToString()).SendAsync("ReceiveJoinNotification", "admin",
                    $"{_shared.connections[Context.ConnectionId].Username} has joined the chat in {isRoom.Id.ToString()}.");
            }
           
        }

    }
}
