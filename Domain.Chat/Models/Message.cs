using System;
using System.Collections.Generic;

namespace Domain.Chat.Models;

public partial class Message
{
    public Guid Id { get; set; }

    public Guid? RoomId { get; set; }

    public string? Contents { get; set; }

    public Guid? SenderId { get; set; }

    public DateTime? InsDate { get; set; }

    public virtual Room? Room { get; set; }

    public virtual Account? Sender { get; set; }
}
