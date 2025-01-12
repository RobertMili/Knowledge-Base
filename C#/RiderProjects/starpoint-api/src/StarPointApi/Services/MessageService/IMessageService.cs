using StarPointApi.Repository.Models;
using System.Threading.Tasks;

namespace StarPointApi.Services.MessageService
{
    public interface IMessageService
    {
        public Task PublishPostMessageAsync(StarPointEntity starPointEntity);
        public Task PublishPutMessageAsync(StarPointEntity starPointEntity, int oldStarpoints);
        public Task PublishDeleteMessageAsync(StarPointEntity starPointEntity);

    }
}
