using Chat_Room_Demo.Models;
using Domain.Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chat_Room_Demo.Hubs
{
    public class ChatOne : Hub
    {
        private readonly ChatRoomContext _context;

        public ChatOne(ChatRoomContext context)
        {
            _context = context;
        }

        private static Dictionary<string, List<string>> roomUsers = new();

        public async Task StartChatWithStaff(Guid customerId, Guid saleId, string initialMessage)
        {
            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.CustomerId == customerId && r.SaleId == saleId);

            if (existingRoom != null)
            {
                await JoinRoom(existingRoom.Id, customerId.ToString());
                await SendMessageToRoom(existingRoom.Id, customerId.ToString(), initialMessage);
            }
            else
            {
                var room = new Room
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    SaleId = saleId,
                    InsDate = DateTime.Now,
                };

                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();

                await Clients.User(saleId.ToString()).SendAsync("ReceiveRoomNotification", room.Id.ToString());
                await SendMessageToRoom(room.Id, customerId.ToString(), initialMessage);
            }
        }

        public async Task JoinRoom(Guid roomId, string username)
        {
            if (!roomUsers.ContainsKey(roomId.ToString()))
                roomUsers[roomId.ToString()] = new List<string>();

            roomUsers[roomId.ToString()].Add(Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("UserJoined", username);
        }

        public async Task SendMessageToRoom(Guid roomId, string user, string message)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", user, message, roomId);
            }
        }
    }
}
