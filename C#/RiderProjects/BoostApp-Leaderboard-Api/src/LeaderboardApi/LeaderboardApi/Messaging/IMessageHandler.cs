using Azure.Messaging.ServiceBus;

namespace LeaderboardApi.Messaging;

public interface IMessageHandler
{
    Task Handle(ProcessMessageEventArgs args);
    Task ErrorHandle(ProcessErrorEventArgs args);
}