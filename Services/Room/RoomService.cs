using Domain.Chat.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat_Room_Demo.Services
{
    public class RoomService : IRoomService
    {
        private readonly ChatRoomContext _context;

        public RoomService(ChatRoomContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetRoomsByStaffIdAsync(Guid staffId)
        {
            return await Task.Run(() =>
            {
                return _context.Rooms
                    .Where(r => r.SaleId == staffId)
                    .ToList();
            });
        }

        public async Task<List<object>> GetWaitingRoomsAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => !_context.Rooms.Any(r2 => r2.CustomerId == r.SaleId))
                .Select(r => new { r.Id })
                .ToListAsync();

            return rooms.Cast<object>().ToList();
        }
    }
}
