namespace Chat_Room_Demo.Services
{
    public interface IChatService
    {
        Task<Guid> JoinRoomPrivate(Guid accountId1, Guid accountId2, string connectionId);
        Task NotifyNewRoom(Guid roomId, Guid saleId, Guid customerId);
    }
}
