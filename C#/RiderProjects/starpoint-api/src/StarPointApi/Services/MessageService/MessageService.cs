using BoostApp.ClassLibrary;
using BoostApp.Shared.Messaging;
using Microsoft.Extensions.Logging;
using StarPointApi.Repository.Models;
using System;
using System.Threading.Tasks;

namespace StarPointApi.Services.MessageService
{
    // This Service handles all of the messaging in azure service bus
    public class MessageService : IMessageService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<MessageService> _logger;

        // Constructor injection of IMessagePublisher
        public MessageService(IMessagePublisher messagePublisher, ILogger<MessageService> logger)
        {
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        // Publish a message for when a new StarPointEntity is being created
        public async Task PublishPostMessageAsync(StarPointEntity starPointEntity)
        {
            // Creating a leaderboard message Model and assigning it values
            var leaderboardMessage = new LeaderboardMessageModel
            {
                Request = RequestEnum.POST,
                Sender = SenderEnum.StarpointAPI,
                TeamMemberId = Guid.Parse(starPointEntity.UserID),
                Starpoints = starPointEntity.StarPoints,
                CreatedDate = starPointEntity.CreatedDate.ToUniversalTime(),
                StarpointId = Guid.Parse(starPointEntity.RowKey)
            };

            try
            {
                // Publish the message using the IMessagePublisher
                await _messagePublisher.Publish(leaderboardMessage);
            }
            catch (Exception ex)
            {
                // If there is an exception it will be logged.
                _logger.LogError("Could not publish message in Post request:", ex.Message);
            }
        }

        // Publish a message for an existing StarPointEntity being deleted
        public async Task PublishDeleteMessageAsync(StarPointEntity starPointEntity)
        {
            var leaderboardMessage = new LeaderboardMessageModel
            {
                Request = RequestEnum.DELETE,
                Sender = SenderEnum.StarpointAPI,
                TeamMemberId = Guid.Parse(starPointEntity.UserID),
                Starpoints = starPointEntity.StarPoints,
                CreatedDate = starPointEntity.CreatedDate,
                StarpointId = Guid.Parse(starPointEntity.RowKey)
            };

            try
            {
                // Publish the message using the IMessagePublisher
                await _messagePublisher.Publish(leaderboardMessage);
            }
            catch (Exception ex)
            {
                // If there is an exception it will be logged.
                _logger.LogError("Could not publish message in Delete request:", ex.Message);
            }
        }

        // Publish a message for an existing StarPointEntity being updated
        public async Task PublishPutMessageAsync(StarPointEntity starPointEntity, int oldStarpoints)
        {
            var newStarpoints = starPointEntity.StarPoints;
            // Calculate whether the new StarPoints value is greater than the old one
            var add = newStarpoints > oldStarpoints;

            var leaderboardMessage = new LeaderboardMessageModel
            {
                Request = RequestEnum.PUT,
                Sender = SenderEnum.StarpointAPI,
                TeamMemberId = Guid.Parse(starPointEntity.UserID),
                Starpoints = Math.Abs(oldStarpoints - newStarpoints),
                CreatedDate = starPointEntity.CreatedDate,
                Add = add,
                StarpointId = Guid.Parse(starPointEntity.RowKey),
            };

            try
            {
                // Publish the message using the IMessagePublisher
                await _messagePublisher.Publish(leaderboardMessage);
            }
            catch (Exception ex)
            {
                // If there is an exception it will be logged.
                _logger.LogError("Could not publish message in Put request:", ex.Message);
            }
        }
    }
}