using Microsoft.EntityFrameworkCore;
using TourMate.MessageService.Repositories.Context;
using TourMate.MessageService.Repositories.GenericRepository;
using TourMate.MessageService.Repositories.IRepositories;
using TourMate.MessageService.Repositories.Models;
using TourMate.MessageService.Repositories.ResponseModels;

namespace TourMate.MessageService.Repositories.Repositories
{
    public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
    {
        public ConversationRepository(TourMateMessageContext context) : base(context)
        {
        }

        public async Task<Conversation?> GetConversationBetweenUsersAsync(int userId1, int userId2)
        {
            return await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.Account1Id == userId1 && c.Account2Id == userId2) ||
                    (c.Account1Id == userId2 && c.Account2Id == userId1));
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation?> GetConversationAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        }

        public async Task<Conversation?> GetConversationByAccountsAsync(int account1Id, int account2Id)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    (c.Account1Id == account1Id && c.Account2Id == account2Id) ||
                    (c.Account1Id == account2Id && c.Account2Id == account1Id)
                );
        }

        public async Task<List<Conversation>> GetByUserIdAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.Account1Id == userId || c.Account2Id == userId)
                .ToListAsync();
        }



        public async Task<List<Message>> GetMessagesByConversationAsync(int conversationId, int page, int pageSize)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderByDescending(m => m.SendAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.MessageType)
                .Include(m => m.File)
                .ToListAsync();
        }

        // Check if there are more messages for pagination
        public async Task<bool> AnyMoreMessagesAsync(int conversationId, DateTime lastMessageTimestamp)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SendAt < lastMessageTimestamp && !m.IsDeleted)
                .AnyAsync();
        }
    }
}