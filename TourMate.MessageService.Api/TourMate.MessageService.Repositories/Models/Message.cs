﻿using System;
using System.Collections.Generic;

namespace TourMate.MessageService.Repositories.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public int SenderId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime SendAt { get; set; }

    public bool IsRead { get; set; }

    public bool IsEdited { get; set; }

    public bool IsDeleted { get; set; }

    public int MessageTypeId { get; set; }

    public int? FileId { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual FileStorage? File { get; set; }

    public virtual MessageType MessageType { get; set; } = null!;
}
