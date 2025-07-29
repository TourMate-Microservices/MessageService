using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourMate.MessageService.Repositories.ResponseModels;
using TourMate.MessageService.Services.GrpcClients;
using TourMate.MessageService.Services.IServices;

namespace TourMate.MessageService.Api.Controllers
{
    [Authorize]
    [Route("api/conversations")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IUserServiceGrpcClient _userServiceGrpcClient;


        public ConversationController(IConversationService conversationService, IUserServiceGrpcClient userServiceGrpcClient)
        {
            _conversationService = conversationService;
            _userServiceGrpcClient = userServiceGrpcClient;
        }

        [HttpGet("fetch-or-create")]
        public async Task<IActionResult> FetchOrCreateConversation([FromQuery] int currentUserId, [FromQuery] int otherUserId, [FromQuery] int currentRole)
        {
            // 1. Lấy hoặc tạo conversation giữa 2 user
            var conversation = await _conversationService.GetOrCreateConversationAsync(currentUserId, otherUserId, currentRole);

            // 2. Lấy info cho account1 người dùng
            string accountName1 = "Người dùng";
            string account1Img = "";

            // 3. Lấy info cho account2 đối phương
            string accountName2 = "Người dùng";
            string account2Img = "";
            if (currentRole == 2)
            {
                var customer = await _userServiceGrpcClient.GetUserByIdAndRoleAsync(currentUserId);
                accountName1 = customer.FullName;
                account1Img = customer.Image;
                var tourGuide = await _userServiceGrpcClient.GetUserByIdAndRoleAsync(otherUserId); // Lấy thông tin TourGuide
                if (tourGuide != null)
                {
                    accountName2 = tourGuide.FullName;
                    account2Img = tourGuide.Image;
                }
            }

            else if (currentRole == 3)
            {
                var tourGuide = await _userServiceGrpcClient.GetUserByIdAndRoleAsync(currentUserId);
                accountName1 = tourGuide.FullName;
                account1Img = tourGuide.Image;
                var customer = await _userServiceGrpcClient.GetUserByIdAndRoleAsync(otherUserId); // Lấy thông tin Customer
                if (customer != null)
                {
                    accountName2 = customer.FullName;
                    account2Img = customer.Image;
                }
            }

            // 6. Trả về dữ liệu dạng đúng kiểu ConversationResponse
            var response = new ConversationResponse
            {
                Conversation = conversation,
                AccountName1 = accountName1,
                AccountName2 = accountName2,
                LatestMessage = null,
                IsRead = false,
                Account2Img = account2Img
            };

            return Ok(response);
        }



        [HttpGet]
        public async Task<IActionResult> GetConversations(
       [FromQuery] int userId,
       [FromQuery] string? searchTerm,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 20)
        {
            if (userId <= 0)
                return BadRequest("Invalid userId");
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be positive");

            var result = await _conversationService.GetConversationsByUserIdAsync(userId, searchTerm ?? "", page, pageSize);

            return Ok(new
            {
                conversations = result.Conversations,
                totalCount = result.TotalCount,
                HasMore = result.TotalCount > page * pageSize
            });
        }

        // Lấy tin nhắn theo ConversationId với phân trang
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 2)
        {
            var (messages, hasMore) = await _conversationService.GetMessagesAsync(conversationId, page, pageSize);

            // Giúp map senderId trong từng message thành senderName
            var messagesWithSenderName = new List<object>();

            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");


            foreach (var message in messages)
            {
                string senderName = "Người dùng";
                string senderAvatarUrl = "";

                var sender = await _userServiceGrpcClient.GetSenderRoleAsync(message.SenderId);

                if (sender != null)
                {
                    if (sender.RoleId == 2)
                    {
                        var customer = await _userServiceGrpcClient.GetUserByIdAndRoleAsync(message.SenderId);
                        if (customer != null)
                            senderName = customer.FullName;
                        senderAvatarUrl = customer.Image;
                    }
                    else if (sender.RoleId == 3)
                    {
                        var tourGuide = await _userServiceGrpcClient.GetUserByIdAndRoleAsync(message.SenderId);
                        if (tourGuide != null)
                            senderName = tourGuide.FullName;
                        senderAvatarUrl = tourGuide.Image;

                    }
                }

                // ✅ Convert thời gian sang giờ Việt Nam
                var sendAtVN = TimeZoneInfo.ConvertTimeFromUtc(message.SendAt, vnTimeZone);

                messagesWithSenderName.Add(new
                {
                    message.MessageId,
                    message.ConversationId,
                    message.MessageText,
                    SendAt = sendAtVN, // ⚠️ Gán thời gian đã chuyển múi giờ
                    message.SenderId,
                    senderName,
                    senderAvatarUrl,
                    message.File?.FileName,
                    message.File?.DownloadUrl,
                });
            }

            return Ok(new
            {
                messages = messagesWithSenderName,
                hasMore
            });
        }
    }
}