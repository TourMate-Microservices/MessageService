﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourMate.MessageService.Repositories.Models;
using TourMate.MessageService.Services.IServices;
using TourMate.MessageService.Repositories.RequestModels;

namespace TourMate.MessageService.Api.Controllers
{
    [Route("api/v1/messages")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessagesService _messageService;

        public MessageController(IMessagesService messageService)
        {
            _messageService = messageService;
        }

        //[HttpGet("{conversationId}")]
        //public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        //{
        //    var (messages, hasMore) = await _messageService.GetMessagesAsync(conversationId, page, pageSize);

        //    return Ok(new
        //    {
        //        messages,
        //        hasMore
        //    });
        //}

        [HttpGet("{id}")]
        public ActionResult<Message> Get(int id)
        {
            return Ok(_messageService.GetMessages(id));
        }

        [HttpGet]
        public ActionResult<IEnumerable<Message>> GetAll([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 1)
        {
            return Ok(_messageService.GetAll(pageSize, pageIndex));
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] MessageCreateModel data)
        {
            var message = data.Convert();
            await _messageService.CreateMessages(message);
            return CreatedAtAction(nameof(Get), new { id = message.MessageId }, message);
        }
        [HttpPost("with-file")]
        public async Task<IActionResult> CreateMessage([FromBody] MessageWithFile data)
        {
            var message = data.Convert();
            await _messageService.CreateMessageWithFile(message);
            return CreatedAtAction(nameof(Get), new { id = message.MessageId }, message);
        }

        // POST: api/messages/{conversationId}/mark-read
        [HttpPost("{conversationId}/mark-read")]
        public async Task<IActionResult> MarkRead(int conversationId, int userId)
        {
            await _messageService.MarkConversationAsRead(conversationId, userId);

            return Ok(new { message = "Đã đánh dấu đọc" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _messageService.DeleteMessages(id);
            return result ? NoContent() : NotFound();
        }
    }
}