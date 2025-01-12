using BoostApp.ClassLibrary;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LeaderboardApi.Services
{
    public class CompetitionMessageService : ICompetitionMessageService
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamRepository;

        public CompetitionMessageService(IGenericRepository<TeamLeaderboardEntity> teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<bool> HandleMessageAsync(LeaderboardMessageModel message)
        {
            if (message.Request == RequestEnum.PUT)
            {
                int competitionId = message.CompetitionId ?? throw new ArgumentException("CompetitionId is null");
                DateTime newEndDate = message.EndDate ?? throw new ArgumentException("NewEndDate is null");
                await UpdateEndDateForTeamsAsync(competitionId, newEndDate);
                return true;
            }
            return false;
        }

        public async Task UpdateEndDateForTeamsAsync(int competitionId, DateTime newEndDate)
        {
            var teams = await _teamRepository.Get(t => t.PartitionKey == competitionId.ToString());

            foreach (var team in teams)
            {
                team.EndDate = newEndDate;
                await _teamRepository.Update(team);
            }
        }
    }
}
