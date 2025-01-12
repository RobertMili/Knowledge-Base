using BoostApp.ClassLibrary;
using System;
using System.Threading.Tasks;

namespace LeaderboardApi.Services.Interfaces
{
    public interface ITeamMessageService : IMessageService
    {
        Task<bool> Post(LeaderboardMessageModel model);
        Task<bool> Put(LeaderboardMessageModel model);
        Task<bool> Delete(Guid teamId, int competitionId);
    }
}
