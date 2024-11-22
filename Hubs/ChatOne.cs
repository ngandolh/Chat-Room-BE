using Chat_Room_Demo.Models;
using Domain.Chat.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
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

        //public async Task SendMessageToRoom(Guid roomId, string user, string message)
        //{
        //    Guid persionId;
        //    var room = await _context.Rooms.FindAsync(roomId);
        //    if (room != null)
        //    {
        //        if (user == "Staff") persionId = Guid.Parse("BFA97975-1915-46A0-B185-ED881C8C953F");
        //        else persionId = Guid.Parse("C9993BBF-9125-466D-BECA-E69CE3DE4A36");
        //        var newMessage = new Message
        //        {
        //            Id = Guid.NewGuid(),
        //            RoomId = room.Id,
        //            Contents = message,
        //            SenderId = persionId,
        //            InsDate = DateTime.Now
        //        };
        //        await _context.Messages.AddAsync(newMessage);
        //        await _context.SaveChangesAsync();


        //        await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", user, message, roomId);
        //    }
        //}

        public async Task SendMessageToRoom(Guid roomId, string user, string message)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room != null)
                {
                    // Tìm `SenderId` dựa trên thông tin người dùng (nên truyền vào `staffId` hoặc `customerId` từ phía client)
                    Guid senderId;

                    if (user == "Staff")
                    {
                        // Kiểm tra ID của Staff (cần truyền `staffId` từ client)
                        senderId = Guid.Parse("BFA97975-1915-46A0-B185-ED881C8C953F"); // Thay thế bằng ID của staff hiện tại
                    }
                    else
                    {
                        // Kiểm tra ID của Customer (cần truyền `customerId` từ client)
                        senderId = Guid.Parse("C9993BBF-9125-466D-BECA-E69CE3DE4A36"); // Thay thế bằng ID của customer hiện tại
                    }

                    // Tạo tin nhắn mới
                    var newMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        RoomId = room.Id,
                        Contents = message,
                        SenderId = senderId,
                        InsDate = DateTime.Now
                    };

                    // Lưu tin nhắn vào database
                    await _context.Messages.AddAsync(newMessage);
                    var saveResult = await _context.SaveChangesAsync();

                    // Kiểm tra lưu thành công hay không
                    if (saveResult > 0)
                    {
                        // Gửi tin nhắn đến group
                        await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", user, message, roomId);
                    }
                    else
                    {
                        // Ghi log nếu lưu thất bại
                        Console.WriteLine("Failed to save message to database");
                    }
                }
                else
                {
                    Console.WriteLine("Room not found");
                }
            }catch(Exception ex) { Console.WriteLine(ex.ToString()); }
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var room in roomUsers)
            {
                room.Value.Remove(Context.ConnectionId);
                if (room.Value.Count == 0)
                    roomUsers.Remove(room.Key);
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
