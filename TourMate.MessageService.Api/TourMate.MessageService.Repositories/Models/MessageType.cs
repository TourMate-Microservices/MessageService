using System;
using System.Collections.Generic;

namespace TourMate.MessageService.Repositories.Models;

public partial class MessageType
{
    public int MessageTypeId { get; set; }

    public string TypeName { get; set; } = null!;
}
