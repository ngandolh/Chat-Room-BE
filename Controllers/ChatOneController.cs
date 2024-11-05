using Chat_Room_Demo.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat_Room_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatOneController : ControllerBase
    {
        private readonly IHubContext<ChatOne> _chatHubContext;

        public ChatOneController(IHubContext<ChatOne> chatHubContext)
        {
            _chatHubContext = chatHubContext;
        }

        [HttpPost("start-chat")]
        public async Task<IActionResult> StartChat(Guid customerId, Guid saleId, string initialMessage)
        {
            await _chatHubContext.Clients.All.SendAsync("StartChatWithStaff", customerId, saleId, initialMessage);
            return Ok("Chat started");
        }

        [HttpPost("join-room")]
        public async Task<IActionResult> JoinRoom(string roomId, string username)
        {
            await _chatHubContext.Clients.All.SendAsync("JoinRoom", roomId, username);
            return Ok("Joined room");
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage(string roomId, string user, string message)
        {
            await _chatHubContext.Clients.All.SendAsync("SendMessageToRoom", roomId, user, message);
            return Ok("Message sent");
        }
    }
}
