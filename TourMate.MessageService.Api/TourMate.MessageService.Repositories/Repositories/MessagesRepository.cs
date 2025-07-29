﻿using Microsoft.EntityFrameworkCore;
using TourMate.MessageService.Repositories.Context;
using TourMate.MessageService.Repositories.GenericRepository;
using TourMate.MessageService.Repositories.IRepositories;
using TourMate.MessageService.Repositories.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using TourMate.MessageService.Repositories.Models;

namespace TourMate.MessageService.Repositories.Repositories
{
    public class MessagesRepository : GenericRepository<Message>, IMessagesRepository
    {
        public MessagesRepository(TourMateMessageContext context) : base(context)
        {
        }

        public async Task<Dictionary<int, DateTime>> GetLatestSendAtsAsync(List<int> conversationIds)
        {
            return await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId))
                .GroupBy(m => m.ConversationId)
                .Select(g => new { g.Key, LatestSendAt = g.Max(m => m.SendAt) })
                .ToDictionaryAsync(g => g.Key, g => g.LatestSendAt);
        }

        public async Task<List<Message>> GetLatestMessagesAsync(List<int> conversationIds)
        {
            return await _context.Messages
                .Where(m => conversationIds.Contains(m.ConversationId))
                .GroupBy(m => m.ConversationId)
                .Select(g => g.OrderByDescending(m => m.SendAt).FirstOrDefault())
                .ToListAsync();
        }

        // Lấy danh sách tin nhắn của một cuộc trò chuyện
        public async Task<List<Message>> GetMessagesAsync(int conversationId, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SendAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.MessageType) // Include message type info if needed
                .ToListAsync();
        }

        public async Task<bool> AnyMoreMessagesAsync(int conversationId, DateTime lastMessageTimestamp)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SendAt < lastMessageTimestamp && !m.IsDeleted)
                .AnyAsync();
        }

        // Thêm một tin nhắn mới
        public async Task AddMessageAsync(int senderId, int conversationId, string messageText, int messageTypeId)
        {
            var message = new Message
            {
                SenderId = senderId,
                ConversationId = conversationId,
                MessageText = messageText,
                MessageTypeId = messageTypeId,
                SendAt = DateTime.UtcNow,
                IsRead = false,
                IsEdited = false,
                IsDeleted = false
            };

            await _context.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        // Cập nhật trạng thái tin nhắn (ví dụ: đánh dấu đã đọc)
        public async Task MarkMessagesAsReadAsync(int conversationId, int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId == userId && !m.IsRead)
                .ToListAsync();

            if (!messages.Any()) return;

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        // Xóa tin nhắn (soft delete)
        public async Task SoftDeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.IsDeleted = true;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Message> CreateMessageWithFile(Message messages)
        {
            try
            {
                var file = messages.File;
                _context.FileStorages.Add(file);
                _context.Messages.Add(messages);
                await _context.SaveChangesAsync();
                return messages;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
