using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BoostApp.ClassLibrary;
using LeaderboardApi.Services.Interfaces;

namespace LeaderboardApi.Messaging
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IMessageService _teamMemberMessageService;
        private readonly IMessageService _teamMessageService;

        private readonly IMessageService _starpointMessageService;

        private readonly IMessageService _competitionMessageService;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(ITeamMemberMessageService teamMemberMessageService,
            ITeamMessageService teamMessageService, IStarpointMessageService starpointMessageService,
            ICompetitionMessageService competitionMessageService,
            ILogger<MessageHandler> logger)
        {
            _teamMemberMessageService = teamMemberMessageService;
            _teamMessageService = teamMessageService;
            _starpointMessageService = starpointMessageService;
            _competitionMessageService = competitionMessageService;
            _logger = logger;
        }

        // Handles the messages that are recieved from LeaderboardQueue
        public async Task Handle(ProcessMessageEventArgs args)
        {
            _logger.LogInformation("Starting to process message id: {id}", args.Message.MessageId);
            var jsonString = Encoding.UTF8.GetString(args.Message.Body);
            LeaderboardMessageModel messageModel = JsonSerializer.Deserialize<LeaderboardMessageModel>(jsonString);

            IMessageService messageService = GetMessageServiceAsync(messageModel);
            if (messageService == null)
            {
                var ex = new NullReferenceException("Sender property should not be null");
                _logger.LogError(ex, "Abandoning message id: {id}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
            }

            var successful = await messageService.HandleMessageAsync(messageModel);
            if (successful)
            {
                _logger.LogInformation("Completing message id: {id}", args.Message.MessageId);
                await args.CompleteMessageAsync(args.Message);
            }

            else
            {
                _logger.LogError("Abandoning message id: {id}", args.Message.MessageId);
                await args.AbandonMessageAsync(args.Message);
            }
        }

        public IMessageService? GetMessageServiceAsync(LeaderboardMessageModel model)
        {
            switch (model.Sender)
            {
                case SenderEnum.StarpointAPI:
                    return _starpointMessageService;

                case SenderEnum.TeamAPI:
                    return _teamMessageService;

                case SenderEnum.TeamMemberAPI:
                    return _teamMemberMessageService;
                case SenderEnum.CompetitionAPI:
                    return _competitionMessageService;
                default: return null;
            }
        }

        // Handles the errors that are recieved from LeaderboardQueue
        public async Task ErrorHandle(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Exception caught in MessageHandler.ErrorHandle");
        }
    }
}