using Chat_Room_Demo.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat_Room_Demo.Controllers
{
    [ApiController]
    [Route("Chat")]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatController(IHubContext<ChatHub> chatHubContext)
        {
            _chatHubContext = chatHubContext;
        }

        [HttpPost("joinRoom")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
        {
            var chatHub = (ChatHub)_chatHubContext; 
            await chatHub.JoinRoomPrive(request.AccountId1, request.AccountId2);
            return Ok("Joined room successfully.");
        }

        [HttpPost("joinTest")]
        public async Task<IActionResult> JoinRoomNormal ([FromBody] JoinRoomRequest request)
        {
            Console.WriteLine($"Request Data: AccountId1={request.AccountId1}, AccountId2={request.AccountId2}");
            var roomId = Guid.NewGuid().ToString(); 
            var username = "SampleUser";

            // Notify the room
            await _chatHubContext.Clients.Group(roomId).SendAsync("ReceiveJoinNotification", 
                "admin", $"{username} has joined the chat.");

            return Ok("Joined room successfully.");
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnectLocal()
        {
            _chatHubContext.Clients.All.SendAsync("FoodDeleted");
            return Ok("Successfully!!!");
        }
    }

    public class JoinRoomRequest
    {
        public Guid AccountId1 { get; set; }
        public Guid AccountId2 { get; set; }
    }
}
