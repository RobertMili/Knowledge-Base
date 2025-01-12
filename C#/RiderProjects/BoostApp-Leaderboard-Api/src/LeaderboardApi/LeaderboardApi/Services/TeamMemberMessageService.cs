using Azure;
using Azure.Core;
using BoostApp.ClassLibrary;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services.Interfaces;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LeaderboardApi.Services
{
    public class TeamMemberMessageService : ITeamMemberMessageService
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamLeaderboardRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _teamMemberLeaderboardRepository;
        private readonly ILogger<TeamMemberMessageService> _logger;
        private readonly IHttpClientService _httpClientService;
        private readonly IExternalApiService _externalApiService;
        private IGenericRepository<TeamMemberLeaderboardEntity> teamMemberLeaderboardRepository;
        private IGenericRepository<TeamLeaderboardEntity> teamLeaderboardRepository;
        private ILogger<TeamMemberMessageService> logger;

        public TeamMemberMessageService(IGenericRepository<TeamLeaderboardEntity> teamLeaderboardRepository,
            IGenericRepository<TeamMemberLeaderboardEntity> teamMemberLeaderboardRepository,
            ILogger<TeamMemberMessageService> logger,
            IHttpClientService httpClientService,
            IExternalApiService externalApiService)
        {
            _teamLeaderboardRepository = teamLeaderboardRepository;
            _teamMemberLeaderboardRepository = teamMemberLeaderboardRepository;
            _logger = logger;
            _httpClientService = httpClientService;
            _externalApiService = externalApiService;
        }

        public TeamMemberMessageService(IGenericRepository<TeamMemberLeaderboardEntity> teamMemberLeaderboardRepository,
            ILogger<TeamMemberMessageService> logger)
        {
            _teamMemberLeaderboardRepository = teamMemberLeaderboardRepository;
            _logger = logger;
        }

        public TeamMemberMessageService(IGenericRepository<TeamMemberLeaderboardEntity> teamMemberLeaderboardRepository,
            IGenericRepository<TeamLeaderboardEntity> teamLeaderboardRepository,
            IExternalApiService externalApiService,
            ILogger<TeamMemberMessageService> logger)
        {
            _teamMemberLeaderboardRepository = teamMemberLeaderboardRepository;
            _teamLeaderboardRepository = teamLeaderboardRepository;
            _externalApiService = externalApiService;
            _logger = logger;
        }

        // Should return true if successful otherwise false
        public async Task<bool> HandleMessageAsync(LeaderboardMessageModel model)
        {
            if (model.TeamMemberId == null || model.TeamId == null)
            {
                _logger.LogError("TeamMemberId or TeamId is null");
                return false;
            }

            switch (model.Request)
            {
                case RequestEnum.POST:
                    return await AddTeamMember(model);
                case RequestEnum.PUT:
                    return await UpdateTeamMember(model);
                case RequestEnum.DELETE:
                    return await DeleteTeamMember((Guid) model.TeamId, (Guid) model.TeamMemberId);
                default:
                    _logger.LogError("Error in HandleMessageAsync in teamMemberService");
                    return false;
            }
        }

        public async Task<bool> AddTeamMember(LeaderboardMessageModel model)
        {
            try
            {
                // Creating the entity with values
                var entity = new TeamMemberLeaderboardEntity
                {
                    RowKey = model.TeamMemberId.ToString(),
                    PartitionKey = model.TeamId.ToString(),
                    Name = model.TeamMemberName,
                    CreatedDate = DateTime.UtcNow
                };

                //Get competitionId from leaderboardDB
                var teams = await _teamLeaderboardRepository.Get(team => team.RowKey == model.TeamId.ToString());
                // Team.PartitionKey is equal to CompetitionId. Get the competitionId from the list and convert it to an int
                var competitionId = teams.FirstOrDefault()?.PartitionKey;
                // Get competitioninfo 
                var competition = await _externalApiService.GetCompetition(Convert.ToInt32(competitionId));

                if (competition != null && competition.StartDate <= DateTime.UtcNow)
                {
                    var startDate = competition.StartDate;
                    // Get starpoint in the TeamMember if they exist
                    var starpoints = await _externalApiService.GetExistingStarpointsAsync(startDate, model.TeamMemberId.ToString());
                    // If the teamMember has starpoints the entity will contain starpoints that will be added when posting teamMember to DB 
                    if (starpoints != null)
                    {
                        entity.Starpoints = starpoints;
                    }
                }

                var response = await _teamMemberLeaderboardRepository.Add(entity);
                // If response from adding the entity is error, false will be returned
                if (response.IsError)
                {
                    _logger.LogError("Error when adding a new teamMember. Database responded with status: {status}", response.Status);
                    return false;
                }
                // If all goes as planned, true will be returned
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in TeamMemberMessageService.AddTeamMember");
                return false;
            }
        }



        public async Task<bool> UpdateTeamMember(LeaderboardMessageModel model)
        {
            // Fetches the Entity to be updated
            var existingEntity =
                await _teamMemberLeaderboardRepository.GetById(model.TeamId.ToString(), model.TeamMemberId.ToString());

            if (existingEntity == null)
            {
                _logger.LogError("No Entity with Rowkey: {id} exists", model.TeamMemberId);
                // If the entity does not exist the method will return false
                return false;
            }

            try
            {
                // Updates the existing entities properties
                existingEntity.Name = model.TeamMemberName;
                existingEntity.PartitionKey = model.TeamId.ToString();
                existingEntity.RowKey = model.TeamMemberId.ToString();

                var response = await _teamMemberLeaderboardRepository.Update(existingEntity);
                // If response from updating the entity is error, false will be returned
                if (response.IsError)
                {
                    _logger.LogError("Error when updating a teamMember. Database responded with status: {status}",
                        response.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in TeamMemberMessageService.UpdateTeamMember");
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteTeamMember(Guid teamId, Guid teamMemberId)
        {
            try
            {
                var response =
                    await _teamMemberLeaderboardRepository.Delete(teamId.ToString(), teamMemberId.ToString());
                // If response when trying to delete a teamMember is Error false will be returned to the MessageHandler
                if (response.IsError)
                {
                    _logger.LogError("Error when deleting teamMember. Database responded with status: {status}",
                        response.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in TeamMemberMessageService.DeleteTeamMember");
                return false;
            }

            return true;
        }
    }
}