using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Models;
using Domain.Chat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System;
using Chat_Room_Demo.Hubs;

namespace Chat_Room_Demo.Services
{
    public class ChatService : IChatService
    {
        private readonly SharedDb _shared;
        private readonly ChatRoomContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatService(SharedDb shared, ChatRoomContext context, IHubContext<ChatHub> hubContext)
        {
            _shared = shared;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Guid> JoinRoomPrivate(Guid accountId1, Guid accountId2, string connectionId)
        {
            var isRoom = await _context.Rooms.FirstOrDefaultAsync(r =>
                (r.CustomerId == accountId1));

            Guid roomId;

            if (isRoom == null && accountId2.ToString() == "00000000-0000-0000-0000-000000000000")
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
                roomId = newRoom.Id;

                // Notify Sales Staff about the new room
                await _hubContext.Clients.All.SendAsync("ReceiveNewRoomNotification", roomId, accountId2, accountId1);

                await _hubContext.Groups.AddToGroupAsync(connectionId, roomId.ToString());

                var username = GetSampleUsername(accountId1);
                _shared.connections[connectionId] = new UserConnection
                {
                    Username = username,
                    ChatRoom = roomId.ToString(),
                };

                await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ReceiveJoinNotification", "admin",
                    $"{_shared.connections[connectionId].Username} has joined the chat in {roomId}.");
            }
            else if (accountId1.ToString() == "00000000-0000-0000-0000-000000000000" && accountId2 != null)
            {
                var roomStaff = await _context.Rooms.FirstOrDefaultAsync(r => r.SaleId == accountId2);
                roomId = roomStaff.Id;
                await _hubContext.Groups.AddToGroupAsync(connectionId, roomStaff.Id.ToString());

                var username = GetSampleUsername(accountId1);
                _shared.connections[connectionId] = new UserConnection
                {
                    Username = username,
                    ChatRoom = roomStaff.Id.ToString(),
                };

                await _hubContext.Clients.Group(roomStaff.Id.ToString()).SendAsync("ReceiveJoinNotification", "sales staff",
                    $"{_shared.connections[connectionId].Username} has joined the chat in {roomStaff.Id}.");
            }
            else
            {
                roomId = isRoom.Id;

                await _hubContext.Groups.AddToGroupAsync(connectionId, roomId.ToString());

                var username = GetSampleUsername(accountId1);
                _shared.connections[connectionId] = new UserConnection
                {
                    Username = username,
                    ChatRoom = roomId.ToString(),
                };

                await _hubContext.Clients.Group(roomId.ToString()).SendAsync("ReceiveJoinNotification", "admin",
                    $"{_shared.connections[connectionId].Username} has joined the chat in {roomId}.");
            }

            return roomId;
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

        public async Task NotifyNewRoom(Guid roomId, Guid saleId, Guid customerId)
        {
            await _hubContext.Clients.User(saleId.ToString()).SendAsync("ReceiveNewRoomNotification", roomId, customerId);
        }
    }
}
