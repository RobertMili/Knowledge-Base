using BoostApp.ClassLibrary;

namespace LeaderboardApi.Services.Interfaces
{
    public interface IMessageService
    {
        public Task<bool> HandleMessageAsync(LeaderboardMessageModel model);
    }
}
