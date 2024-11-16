using Chat_Room_Demo.Services;
using Domain.Chat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat_Room_Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        /// <summary>
        /// Get a list of chat rooms assigned to a specific staff member (SaleId).
        /// </summary>
        /// <param name="staffId">The ID of the staff member (SaleId).</param>
        /// <returns>A list of chat rooms associated with the staff member.</returns>
        /// <response code="200">Returns the list of rooms for the staff.</response>
        /// <response code="404">If no rooms are found for the given staffId.</response>
        [HttpGet("ListRoomsForStaff")]
        [ProducesResponseType(typeof(List<Room>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListRoomForStaff([FromQuery] Guid staffId)
        {
            var rooms = await _roomService.GetRoomsByStaffIdAsync(staffId);

            if (rooms == null || !rooms.Any())
            {
                return NotFound("No rooms found for the given staff ID.");
            }

            return Ok(rooms);
        }

        [HttpGet("waiting")]
        public async Task<IActionResult> GetWaitingRooms()
        {
            var rooms = await _roomService.GetWaitingRoomsAsync();
            return Ok(rooms);
        }
    }
}
