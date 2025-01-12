using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MembershipApi.Domain.Interfaces;
using MembershipApi.Domain.Repositories.Entities;
using MembershipAPI.Extensions;
using Microsoft.Extensions.Logging;
using Sigma.BoostApp.Contracts;
using Sigma.BoostApp.Contracts.GQL.Mutations;

namespace MembershipApi.Services
{
    public class TeamService
    {
        private readonly CompetitionService _contestService;

        private readonly ITableOfType<TeamEntity> _teamRepository;

        private readonly ILogger<CompetitionService> _logger;

        public TeamService(CompetitionService competitionService, ITableOfType<TeamEntity> teamRepository, ILogger<CompetitionService> logger)
        {
            _contestService = competitionService;
            _teamRepository = teamRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<TeamInfo>> GetAll(string competitionId)
        {
            _logger.LogInformation("GetAll for " + competitionId);
            var teams = await GetTeamEntities(competitionId);
            return teams.Select(x => x.GetMinimalInfo());
        }

        private async Task<TeamInfo> Set(int teamId, AssignTeamToContest data)
        {
            _logger.LogDebug("AssignTeamToContest {@AssignTeamToContest}", data);
            
            var entity = new TeamEntity(data.ContestId, teamId)
            {
                CompetitionId = data.ContestId,
                
                TeamId = teamId.ToString(),
                TeamNumber = teamId,
                TeamName = data.TeamName,
                TeamImageUrl = data.TeamImageUrl,

                GoalDescription = data.GoalDescription,
                GoalPoints = data.GoalPoints,
                GoalImageUrl = data.GoalImageUrl
            };
            
            entity = await _teamRepository.SaveAsync(entity);

            return entity.GetMinimalInfo();
        }

        internal async Task<TeamInfo> GetById(string competitionId, int teamId)
        {
            var team = await GetTeamEntity(competitionId, teamId);
            return team.GetMinimalInfo();
        }

        internal async Task<GoalInfo> GetTeamGoal(string competitionId, int teamId)
        {
            var teamEntity = await _teamRepository.ReadAsync("contest-" + competitionId, teamId.ToString());
            return new GoalInfo
            {
                Description = teamEntity?.GoalDescription ?? "Enter description",
                Score = Math.Max(teamEntity?.GoalPoints ?? 0, 100),
                Image = new ProfileImage
                {
                    Data = teamEntity?.GoalImageUrl ?? "https://www.extension.harvard.edu/professional-development/sites/extension.harvard.edu.professional-development/files/field/image/PDP_goal-setting-hero.jpg",
                    DataType = "text/uri-list"
                }
            };
        }

        internal async Task<TeamInfo> UpdateTeam(int teamId, AssignTeamToContest payload)
        {
            await Set(teamId, payload);
            return await GetById(payload.ContestId, teamId);
        }

        internal async Task<TeamInfo> AddTeam(AssignTeamToContest payload)
        {
            var teams = await GetAll(payload.ContestId);

            var teamId = 0;
            var rnd = new Random();
            do 
            {
                var teamNumber = rnd.Next(1, 10000);
                var competitionNumber = int.Parse(payload.ContestId);
                teamId = competitionNumber + teamNumber;

                _logger.LogInformation("Generated team number {TeamNumber} for competition {competitionNumber}", teamNumber, competitionNumber);

            } while (teams.Any(x => x.Id == teamId.ToString()));

            await Set(teamId, payload);

            return await GetById(payload.ContestId, teamId);
        }

        private async Task<TeamEntity> GetTeamEntity(string competitionId, int teamId)
        {
            return await _teamRepository.ReadAsync("competition-" + competitionId, teamId.ToString());
        }

        public async Task<IEnumerable<TeamEntity>> GetTeamEntities(string competitionId)
        {
            return await _teamRepository.ReadPartitionAsync("competition-" + competitionId);
        }

        internal async Task<TeamEntity> DeleteTeam(string competitionId, int teamid)
        {
            return await _teamRepository.RemoveAsync("competition-" + competitionId, teamid.ToString());
        }
    }
}