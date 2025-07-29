using TourMate.MessageService.Repositories.GenericRepository;
using TourMate.MessageService.Repositories.Models;

namespace TourMate.MessageService.Repositories.IRepositories
{
    public interface IMessagesRepository : IGenericRepository<Message>
    {
        Task<List<Message>> GetLatestMessagesAsync(List<int> conversationIds);
        Task<Dictionary<int, DateTime>> GetLatestSendAtsAsync(List<int> conversationIds);
        Task<List<Message>> GetMessagesAsync(int conversationId, int page, int pageSize);
        Task<bool> AnyMoreMessagesAsync(int conversationId, DateTime lastMessageTimestamp);
        Task AddMessageAsync(int senderId, int conversationId, string messageText, int messageTypeId);
        Task MarkMessagesAsReadAsync(int conversationId, int userId);
        Task SoftDeleteMessageAsync(int messageId);
        Task<Message?> CreateMessageWithFile(Message messages);
    }
}
