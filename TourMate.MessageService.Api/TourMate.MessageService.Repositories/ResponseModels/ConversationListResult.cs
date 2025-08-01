﻿using TourMate.MessageService.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourMate.MessageService.Repositories.ResponseModels
{
    public class ConversationListResult
    {
        public List<ConversationResponse> Conversations { get; set; } = new();
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
    }
}
