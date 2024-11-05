using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Hubs;
using Chat_Room_Demo.Models;
using Domain.Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
            Room room;
            Guid roomId;

            if (accountId2 == Guid.Empty) // Customer initiates the chat
            {
                // Check if there is already a room for the customer
                room = await _context.Rooms.FirstOrDefaultAsync(r => r.CustomerId == accountId1);

                if (room == null)
                {
                    // Create a new room for the customer
                    room = new Room
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = accountId1,
                        SaleId = Guid.Empty, // Placeholder for Sales ID
                        InsDate = DateTime.Now
                    };

                    await _context.Rooms.AddAsync(room);
                    await _context.SaveChangesAsync();
                }
            }
            else // Sales staff initiates or joins an existing chat
            {
                // Check if there is already a room for the sales staff
                room = await _context.Rooms.FirstOrDefaultAsync(r => r.SaleId == accountId2);

                if (room == null)
                {
                    // Create a new room with the sales staff
                    room = new Room
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = Guid.Empty, // Placeholder for Customer ID
                        SaleId = accountId2,
                        InsDate = DateTime.Now
                    };

                    await _context.Rooms.AddAsync(room);
                    await _context.SaveChangesAsync();
                }
            }

            roomId = room.Id;

            // Add user to SignalR group
            await _hubContext.Groups.AddToGroupAsync(connectionId, roomId.ToString());

            var username = GetSampleUsername(accountId1 == Guid.Empty ? accountId2 : accountId1);
            _shared.connections[connectionId] = new UserConnection
            {
                Username = username,
                ChatRoom = roomId.ToString(),
            };

            // Notify the group about the new user joining
            string role = accountId2 == Guid.Empty ? "Customer" : "Sales Staff";
            await _hubContext.Clients.Group(roomId.ToString())
                .SendAsync("ReceiveJoinNotification", role, $"{username} has joined the chat in {roomId}.");

            return roomId;
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

        public async Task NotifyNewRoom(Guid roomId, Guid saleId, Guid customerId)
        {
            await _hubContext.Clients.User(saleId.ToString()).SendAsync("ReceiveNewRoomNotification", roomId, customerId);
        }
    }
}
