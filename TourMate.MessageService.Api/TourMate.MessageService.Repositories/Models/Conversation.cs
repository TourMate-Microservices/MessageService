using System;
using System.Collections.Generic;

namespace TourMate.MessageService.Repositories.Models;

public partial class Conversation
{
    public int ConversationId { get; set; }

    public int Account1Id { get; set; }

    public int Account2Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
