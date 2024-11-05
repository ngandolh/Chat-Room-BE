using Domain.Chat.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;

namespace Chat_Room_Demo.Hubs
{
    public class ChatOne : Hub
    {
        private readonly ChatRoomContext _context;
        public ChatOne(ChatRoomContext context)
        {
            _context = context;
        }

        private static Dictionary<string, List<string>> roomUsers = new Dictionary<string, List<string>>();

        public async Task StartChatWithStaff(Guid customerId, Guid saleId, string initialMessage)
        {
            // Check if a room already exists for this customer and staff
            var existingRoom = await _context.Rooms
                                             .FirstOrDefaultAsync(r => r.CustomerId == customerId && r.SaleId == saleId);

            if (existingRoom != null)
            {
                // Room exists, join it instead of creating a new one
                await JoinRoom(existingRoom.Id, customerId.ToString());

                // Optionally, send the initial message to this existing room
                await SendMessageToRoom(existingRoom.Id, customerId.ToString(), initialMessage);
            }
            else
            {
                // No existing room, create a new one
                var room = new Room
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    SaleId = saleId,
                    InsDate = DateTime.Now,
                };

                await _context.Rooms.AddAsync(room);
                await _context.SaveChangesAsync();

                // Notify staff of the new room
                await Clients.User(saleId.ToString()).SendAsync("ReceiveRoomNotification", room.Id.ToString());

                // Send initial message in the new room
                 await SendMessageToRoom(room.Id, customerId.ToString(), initialMessage);
            }
        }



        public async Task JoinRoom(Guid roomId, string username)
        {
            // Kiểm tra nếu room đã tồn tại và có nhiều nhất 2 người
            if (roomUsers.ContainsKey(roomId.ToString()))
            {
                if (roomUsers[roomId.ToString()].Count >= 2)
                {
                    await Clients.Caller.SendAsync("RoomFull", "This room is full.");
                    return;
                }
                roomUsers[roomId.ToString()].Add(Context.ConnectionId);
            }
            else
            {
                roomUsers[roomId.ToString()] = new List<string> { Context.ConnectionId };
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("UserJoined", username);
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

        public async Task SendMessageToRoom(Guid roomId, string user, string message)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", user, message, roomId);
        }

        public async Task HandleMessage(string action, JObject message)
        {
            // Parse JSON message
            if (action == "StartChatWithStaff")
            {
                var customerId = Guid.Parse(message["data"]["customerId"].ToString());
                var saleId = Guid.Parse(message["data"]["saleId"].ToString());
                var initialMessage = message["data"]["initialMessage"].ToString();

                // Gửi lại cho tất cả các client
                await Clients.All.SendAsync("StartChatWithStaff", customerId, saleId, initialMessage);
            }
            else if (action == "JoinRoom")
            {
                var roomId = message["data"]["roomId"].ToString();
                var username = message["data"]["username"].ToString();
                await JoinRoom(Guid.Parse(roomId), username);
            }
            else if (action == "SendMessageToRoom")
            {
                var roomId = message["data"]["roomId"].ToString();
                var user = message["data"]["user"].ToString();
                var messageText = message["data"]["message"].ToString();
                await SendMessageToRoom(Guid.Parse(roomId), user, messageText);
            }
        }
    }
}
