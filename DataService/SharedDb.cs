using Chat_Room_Demo.Models;
using System.Collections.Concurrent;

namespace Chat_Room_Demo.DataService
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connections = new ConcurrentDictionary<string, UserConnection>();

        public ConcurrentDictionary<string, UserConnection> connections => _connections;
    }
}
