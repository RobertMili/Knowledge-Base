using System;
using System.Threading.Tasks;
using LeaderboardApi.Models;

namespace LeaderboardApi.Services.Interfaces
{
    public interface IExternalApiService
    {
        Task<CompetitionResponseModel> GetCompetition(int competitionId); 
        Task<int> GetExistingStarpointsAsync(DateTime startDate, string userID);
    }
}
