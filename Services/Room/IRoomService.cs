using Domain.Chat.Models;

namespace Chat_Room_Demo.Services
{
    public interface IRoomService
    {
        Task<List<Room>> GetRoomsByStaffIdAsync(Guid staffId);
        Task<List<object>> GetWaitingRoomsAsync();
    }
}
