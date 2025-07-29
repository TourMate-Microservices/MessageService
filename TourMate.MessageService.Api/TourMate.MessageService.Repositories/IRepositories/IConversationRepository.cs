using TourMate.MessageService.Repositories.GenericRepository;
using TourMate.MessageService.Repositories.Models;
using TourMate.MessageService.Repositories.ResponseModels;

namespace TourMate.MessageService.Repositories.IRepositories
{
    public interface IConversationRepository : IGenericRepository<Conversation>
    {
        Task<Conversation?> GetConversationBetweenUsersAsync(int userId1, int userId2);
        Task<Conversation> CreateConversationAsync(Conversation conversation);
        Task<Conversation?> GetConversationAsync(int conversationId);
        Task<Conversation?> GetConversationByAccountsAsync(int account1Id, int account2Id);
        Task<List<Conversation>> GetByUserIdAsync(int userId);
        Task<List<Message>> GetMessagesByConversationAsync(int conversationId, int page, int pageSize);
        Task<bool> AnyMoreMessagesAsync(int conversationId, DateTime lastMessageTimestamp);
    }
}
