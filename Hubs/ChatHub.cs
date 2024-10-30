﻿using Chat_Room_Demo.DataService;
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
    }
}
