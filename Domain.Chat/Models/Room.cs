using System;
using System.Collections.Generic;

namespace Domain.Chat.Models;

public partial class Room
{
    public Guid Id { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? SaleId { get; set; }

    public DateTime? InsDate { get; set; }

    public virtual Account? Customer { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Account? Sale { get; set; }
}
