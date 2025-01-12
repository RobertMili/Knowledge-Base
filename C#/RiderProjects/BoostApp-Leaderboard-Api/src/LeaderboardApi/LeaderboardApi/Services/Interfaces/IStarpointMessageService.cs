using LeaderboardApi.Models;

namespace LeaderboardApi.Services.Interfaces;

public interface IStarpointMessageService : IMessageService
{
    Task<bool> RemoveStarpoints(StarpointLeaderboardModel starpointLeaderboardModel);
    Task<bool> AddStarpoints(StarpointLeaderboardModel starpointLeaderboardModel);
    Task<bool> AddStarpointsToBackup(StarpointLeaderboardModel starpointLeaderboardModel);
    Task<bool> RemoveStarpointsFromBackup(StarpointLeaderboardModel starpointLeaderboardModel);
}