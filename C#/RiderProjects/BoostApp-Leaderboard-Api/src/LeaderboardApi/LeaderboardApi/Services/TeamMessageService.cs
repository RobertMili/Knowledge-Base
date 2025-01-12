using Azure;
using BoostApp.ClassLibrary;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services.Interfaces;

namespace LeaderboardApi.Services
{
    public class TeamMessageService : ITeamMessageService
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _genericTeamRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _genericTeamMemberRepository;
        private readonly ILogger<TeamMessageService> _logger;
        private readonly IExternalApiService _externalApiService;


        public TeamMessageService(IGenericRepository<TeamLeaderboardEntity> genericTeamRepository,
            IGenericRepository<TeamMemberLeaderboardEntity> genericTeamMemberRepository,
            ILogger<TeamMessageService> logger,
            IExternalApiService externalApiService)
        {
            _genericTeamRepository = genericTeamRepository;
            _genericTeamMemberRepository = genericTeamMemberRepository;
            _logger = logger;
            _externalApiService = externalApiService;
        }

        // Should return true if successful, otherwise false
        public async Task<bool> HandleMessageAsync(LeaderboardMessageModel model)
        {
            switch (model.Request)
            {
                case RequestEnum.POST:
                    return await Post(model);
                case RequestEnum.PUT:
                    return await Put(model);
                case RequestEnum.DELETE:
                    return await Delete((Guid) model.TeamId, (int) model.CompetitionId);
                default:
                    _logger.LogError("Error in HandleMessageAsync in teamService");
                    return false;
            }
        }

        public async Task<bool> Post(LeaderboardMessageModel model)
        {
            // Call GetCompetitionEndDate to get the competition end date
            var competition = await _externalApiService.GetCompetition((int) model.CompetitionId);
            if (competition == null)
            {
                _logger.LogError("Error when trying to GET competition, competition is null");
                return false;
            }

            // Create a new TeamLeaderboardEntity object with the values from the model
            var entity = new TeamLeaderboardEntity
            {
                RowKey = model.TeamId.ToString(),
                PartitionKey = model.CompetitionId.ToString(),
                Name = model.TeamName,
                ImageUrl = model.TeamImageUrl,
                CreatedDate = DateTime.UtcNow,
                EndDate = DateTime.SpecifyKind(competition.EndDate, DateTimeKind.Utc)
            };

            // Add the new entity to the repository
            var response = await _genericTeamRepository.Add(entity);

            if (response.IsError)
            {
                _logger.LogError("Error when adding a new team. Database responded with status: {status}",
                    response.Status);
                return false;
            }

            return true;
        }


        public async Task<bool> Delete(Guid teamId, int competitionId)
        {
            try
            {
                var teamMembers = await _genericTeamMemberRepository.Get(tm => tm.PartitionKey == teamId.ToString());

                foreach (var teamMemberLeaderboardEntity in teamMembers)
                {
                    var deleteTeamMemberResponse =
                        await _genericTeamMemberRepository.Delete(teamMemberLeaderboardEntity.PartitionKey,
                            teamMemberLeaderboardEntity.RowKey);
                    if (deleteTeamMemberResponse.IsError)
                    {
                        _logger.LogError("Error when deleting TeamMember. Database responded with status: {status}",
                            deleteTeamMemberResponse.Status);
                        return false;
                    }
                }

                // Delete the entity with the given primary key
                var response = await _genericTeamRepository.Delete(competitionId.ToString(), teamId.ToString());
                if (response.IsError)
                {
                    _logger.LogError("Error when deleting team. Database responded with status: {status}",
                        response.Status);
                    return false;
                }

                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in TeamMessageService.Delete");
                return false;
            }
        }

        public async Task<bool> Put(LeaderboardMessageModel model)
        {
            try
            {
                var existingEntity =
                    await _genericTeamRepository.GetById(model.CompetitionId.ToString(), model.TeamId.ToString());

                if (existingEntity != null)
                {
                    // Update the entity with the new data if it is not null or empty
                    if (!string.IsNullOrWhiteSpace(model.TeamName))
                    {
                        existingEntity.Name = model.TeamName;
                    }

                    if (!string.IsNullOrWhiteSpace(model.TeamImageUrl))
                    {
                        existingEntity.ImageUrl = model.TeamImageUrl;
                    }

                    // Update the entity in the repository
                    var response = await _genericTeamRepository.Update(existingEntity);

                    if (response.IsError)
                    {
                        _logger.LogError("Error when updating a team. Database responded with status: {status}",
                            response.Status);
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Cannot update team with ID {id}. Entity not found.", model.TeamId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in TeamMessageService.Put");
                return false;
            }
        }
    }
}