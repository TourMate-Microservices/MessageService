using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourMate.MessageService.Services.Grpc;


namespace TourMate.MessageService.Services.GrpcClients
{
    public interface IUserServiceGrpcClient
    {
        Task<UserInfo?> GetUserByIdAndRoleAsync(int userId);
        Task<SenderRoleResponse> GetSenderRoleAsync(int userId);
    }
    public class UserServiceGrpcClient : IUserServiceGrpcClient    {
        private readonly GrpcChannel _channel;
        private readonly UserService.UserServiceClient _client;
        private readonly ILogger<UserServiceGrpcClient> _logger;

        public UserServiceGrpcClient(IConfiguration configuration, ILogger<UserServiceGrpcClient> logger)
        {
            _logger = logger;

            try
            {
                var userServiceUrl = configuration["GrpcServices:UserService"] ?? "http://user-service:9092";

                _channel = GrpcChannel.ForAddress(userServiceUrl);
                _client = new UserService.UserServiceClient(_channel);

                _logger.LogInformation("UserService gRPC client initialized with URL: {Url}", userServiceUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize gRPC client");
                throw;
            }
        }

        public async Task<SenderRoleResponse> GetSenderRoleAsync(int userId)
        {
            try
            {
                var request = new SenderIdRequest
                { 
                     SenderId = userId,
                };
                var response = await _client.GetSenderRoleAsync(request);
                _logger.LogInformation("GetSenderRoleAsync called for userId: {UserId}, Role: {Role}", userId, response.RoleId);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error in GetSenderRoleAsync");
                throw;
            }
        }


        public async Task<UserInfo?> GetUserByIdAndRoleAsync(int userId)
        {
            try
            {
                var request = new UserIdRequest
                {
                    UserId = userId,
                };

                var response = await _client.GetBasicUserInfoAsync(request);

                return response.User;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error in GetUserByIdAndRoleAsync");
                return null;
            }

        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
