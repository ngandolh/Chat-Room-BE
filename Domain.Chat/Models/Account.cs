using System;
using System.Collections.Generic;

namespace Domain.Chat.Models;

public partial class Account
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Role { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Room> RoomCustomers { get; set; } = new List<Room>();

    public virtual ICollection<Room> RoomSales { get; set; } = new List<Room>();
}
