using StarPointApi.Repository.Models;
using System.Threading.Tasks;

namespace StarPointApi.Services.MessageService
{
    public class MockMessageService : IMessageService
    {
        public Task PublishDeleteMessageAsync(StarPointEntity starPointEntity)
        {
            return Task.CompletedTask;
        }

        public Task PublishPostMessageAsync(StarPointEntity starPointEntity)
        {
            return Task.CompletedTask;
        }

        public Task PublishPutMessageAsync(StarPointEntity starPointEntity, int oldStarpoints)
        {
            return Task.CompletedTask;
        }
    }
}
