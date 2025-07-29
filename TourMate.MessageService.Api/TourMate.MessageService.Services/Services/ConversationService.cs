using TourMate.MessageService.Repositories.IRepositories;
using TourMate.MessageService.Repositories.Models;
using TourMate.MessageService.Repositories.ResponseModels;
using TourMate.MessageService.Services.Grpc;
using TourMate.MessageService.Services.GrpcClients;
using TourMate.MessageService.Services.IServices;

namespace TourMate.MessageService.Services.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository ConversationRepository;
        private readonly IMessagesRepository _messageRepository;
        private readonly IUserServiceGrpcClient _userGrpcClient;

        public ConversationService(IConversationRepository conversationRepo, IMessagesRepository messagesRepository, IUserServiceGrpcClient userServiceGrpcClient)
        {
            ConversationRepository = conversationRepo;
            _messageRepository = messagesRepository;
            _userGrpcClient = userServiceGrpcClient;
        }

        public async Task<Conversation> GetOrCreateConversationAsync(int currentUserId, int userId, int currentRole)
        {
            var conversation = await ConversationRepository.GetConversationBetweenUsersAsync(currentUserId, userId);
            if (conversation != null) return conversation;

            var newConv = new Conversation();
            newConv.CreatedAt = DateTime.UtcNow;

            if (currentRole == 2)
            {
                newConv.Account1Id = userId; // TourGuide is Account1
                newConv.Account2Id = currentUserId; // Customer is Account2
            }

            else if(currentRole == 3)
            {
                newConv.Account1Id = currentUserId; // TourGuide is Account1
                newConv.Account2Id = userId; // Customer is Account2
            }
            else
            {
                throw new ArgumentException("Invalid role for conversation creation");
            }

            return await ConversationRepository.CreateConversationAsync(newConv);
        }


        public async Task<(List<ConversationResponse> Conversations, int TotalCount)> GetConversationsByUserIdAsync(
 int userId, string searchTerm, int page, int pageSize)
        {
            searchTerm = searchTerm?.Trim().ToLower() ?? "";

            var allConversations = await ConversationRepository.GetByUserIdAsync(userId);

            // Lấy danh sách partnerId + roleId
            var userIdRoleList = allConversations
                .Select(c =>
                {
                    int partnerId;
                    int roleId;

                    if (c.Account1Id == userId)
                    {
                        partnerId = c.Account2Id;
                        roleId = 2; // Account2 là Customer
                    }
                    else
                    {
                        partnerId = c.Account1Id;
                        roleId = 3; // Account1 là TourGuide
                    }

                    return (partnerId, roleId);
                })
                .Distinct()
                .ToList();

            // Gọi từng user lấy thông tin qua gRPC
            var userInfoMap = new Dictionary<int, UserInfo>();

            foreach (var (partnerId, roleId) in userIdRoleList)
            {
                if (!userInfoMap.ContainsKey(partnerId))
                {
                    var info = await _userGrpcClient.GetBasicUserInfoAsync(partnerId);
                    if (info != null)
                    {
                        userInfoMap[partnerId] = info;
                    }
                }
            }

            // Lọc theo search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var matchedIds = userInfoMap
                    .Where(kv => kv.Value.FullName.ToLower().Contains(searchTerm))
                    .Select(kv => kv.Key)
                    .ToHashSet();

                allConversations = allConversations
                    .Where(c =>
                    {
                        int partnerId = c.Account1Id == userId ? c.Account2Id : c.Account1Id;
                        return matchedIds.Contains(partnerId);
                    })
                    .ToList();
            }

            // Lấy latest message
            var conversationIds = allConversations.Select(c => c.ConversationId).ToList();
            var latestSendAts = await _messageRepository.GetLatestSendAtsAsync(conversationIds);

            var ordered = allConversations
                .OrderByDescending(c => latestSendAts.ContainsKey(c.ConversationId)
                    ? latestSendAts[c.ConversationId]
                    : c.CreatedAt)
                .ToList();

            var pagedConversations = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedConversationIds = pagedConversations.Select(c => c.ConversationId).ToList();
            var latestMessages = await _messageRepository.GetLatestMessagesAsync(pagedConversationIds);

            var result = pagedConversations.Select(c =>
            {
                var partnerId = c.Account1Id == userId ? c.Account2Id : c.Account1Id;
                var userInfo = userInfoMap.ContainsKey(partnerId) ? userInfoMap[partnerId] : null;

                var latestMessage = latestMessages.FirstOrDefault(m => m.ConversationId == c.ConversationId);
                bool isRead = latestMessage == null || latestMessage.SenderId == userId || latestMessage.IsRead;

                return new ConversationResponse
                {
                    Conversation = new Conversation
                    {
                        ConversationId = c.ConversationId,
                        Account1Id = c.Account1Id,
                        Account2Id = c.Account2Id,
                        CreatedAt = c.CreatedAt
                    },
                    AccountName2 = userInfo?.FullName ?? "Unknown",
                    Account2Img = userInfo?.Image ?? "",
                    LatestMessage = latestMessage,
                    IsRead = isRead
                };
            }).ToList();

            return (result, allConversations.Count);
        }



        public async Task<(List<Message> messages, bool hasMore)> GetMessagesAsync(int conversationId, int page, int pageSize)
        {
            var messages = await ConversationRepository.GetMessagesByConversationAsync(conversationId, page, pageSize);
            var hasMore = messages.Count == pageSize &&
                          await ConversationRepository.AnyMoreMessagesAsync(conversationId, messages.LastOrDefault()?.SendAt ?? DateTime.MinValue);

            return (messages, hasMore);
        }

        public Conversation GetConversation(int id)
        {
            return ConversationRepository.GetById(id);
        }

        public IEnumerable<Conversation> GetAll(int pageSize, int pageIndex)
        {
            return ConversationRepository.GetAll(pageSize, pageIndex);
        }

        public void CreateConversation(Conversation conversation)
        {
            ConversationRepository.Create(conversation);
        }

        public void UpdateConversation(Conversation conversation)
        {
            ConversationRepository.Update(conversation);
        }

        public bool DeleteConversation(int id)
        {
            ConversationRepository.Remove(id);
            return true;
        }
    }
}