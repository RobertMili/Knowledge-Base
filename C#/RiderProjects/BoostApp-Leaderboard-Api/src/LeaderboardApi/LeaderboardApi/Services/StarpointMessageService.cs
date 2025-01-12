using BoostApp.ClassLibrary;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services.Interfaces;

namespace LeaderboardApi.Services
{
    public class StarpointMessageService : IStarpointMessageService
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamLeaderboardRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _teamMemberLeaderboardRepository;
        private readonly IGenericRepository<StarpointEntity> _starpointLeaderboardRepository;
        private readonly ILogger<StarpointMessageService> _logger;

        public StarpointMessageService(IGenericRepository<TeamLeaderboardEntity> teamLeaderboardRepository,
            IGenericRepository<TeamMemberLeaderboardEntity> teamMemberLeaderboardRepository,
            IGenericRepository<StarpointEntity> starpointLeaderboardRepository,
            ILogger<StarpointMessageService> logger)
        {
            _teamLeaderboardRepository = teamLeaderboardRepository;
            _teamMemberLeaderboardRepository = teamMemberLeaderboardRepository;
            _starpointLeaderboardRepository = starpointLeaderboardRepository;
            _logger = logger;
        }

        // Should return true if successful otherwise false
        public async Task<bool> HandleMessageAsync(LeaderboardMessageModel model)
        {
            if (!ModelIsValid(model))
            {
                _logger.LogError("Model is not valid");
                return false;
            }

            var starpointLeaderboardModel = new StarpointLeaderboardModel
            {
                TeamMemberId = (Guid) model.TeamMemberId,
                Starpoints = (int) model.Starpoints,
                CreatedDate = (DateTime) model.CreatedDate,
                StarpointId = model.StarpointId?? new Guid(), 

            };
            switch (model.Request)
            {
                case RequestEnum.POST:
                    return await AddStarPointsinLeaderBoard(starpointLeaderboardModel);
                case RequestEnum.PUT:
                    return await ((bool) model.Add
                        ? AddStarPointsinLeaderBoard(starpointLeaderboardModel)
                        : RemoveStarPointsinLeaderBoard(starpointLeaderboardModel));
                case RequestEnum.DELETE:
                    return await RemoveStarPointsinLeaderBoard(starpointLeaderboardModel);
                default:
                    return false;
            }
        }

        private bool ModelIsValid(LeaderboardMessageModel model)
        {
            if ((model.Request == RequestEnum.PUT && model.Add == null) || model.Starpoints == null ||
                model.TeamMemberId == null || model.CreatedDate == null)
                return false;

            return true;
        }

        public async Task<bool> AddStarPointsinLeaderBoard(StarpointLeaderboardModel starpointLeaderboardModel)
        {
            bool addStarpointsResponse = await AddStarpoints(starpointLeaderboardModel);
            if(addStarpointsResponse) { 
                bool addStarpointsToBackupResponse = await AddStarpointsToBackup(starpointLeaderboardModel);
            }
            return addStarpointsResponse ;
        }

        public async Task<bool> RemoveStarPointsinLeaderBoard(StarpointLeaderboardModel starpointLeaderboardModel)
        {
            bool removeStarpointsResponse = await RemoveStarpoints(starpointLeaderboardModel);
            if(removeStarpointsResponse) { 
                bool removeStarpointsToBackupResponse = await  RemoveStarpointsFromBackup(starpointLeaderboardModel);
            }
            return removeStarpointsResponse ;

        }

        #region Team member starpoint management
        public async Task<bool> RemoveStarpoints(StarpointLeaderboardModel starpointLeaderboardModel)
        {
            var teamMemberToUpdate =
                await GetCorrectTeamMemberLeaderboardEntity(starpointLeaderboardModel);
            if (teamMemberToUpdate == null)
            {
                _logger.LogError(
                    "No Entity with RowKey: {id} are currently active", starpointLeaderboardModel.TeamMemberId);
                return false;
            }

            // Tries to remove the starpoints from the teamMember by using the Update method on the IGenericRepository<TeamMemberLeaderboardEntity>
            try
            {
                teamMemberToUpdate.Starpoints -= starpointLeaderboardModel.Starpoints;
                var teamMemberResponse = await _teamMemberLeaderboardRepository.Update(teamMemberToUpdate);
                if (teamMemberResponse.IsError)
                {
                    _logger.LogError("Database responded with status: {status}", teamMemberResponse.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in StarpointMessageService.RemoveStarpoints");
                return false;
            }

            return true;
        }

        public async Task<bool> AddStarpoints(StarpointLeaderboardModel starpointLeaderboardModel)
        {
            var teamMemberToUpdate =
                await GetCorrectTeamMemberLeaderboardEntity(starpointLeaderboardModel);
            if (teamMemberToUpdate == null)
            {
                _logger.LogError(
                    "No Entity with RowKey: {id} are currently active", starpointLeaderboardModel.TeamMemberId);
                return false;
            }

            // Tries to add the starpoints from the teamMember by using the Update method on the IGenericRepository<TeamMemberLeaderboardEntity>
            try
            {
                teamMemberToUpdate.Starpoints += starpointLeaderboardModel.Starpoints;
                var teamMemberResponse = await _teamMemberLeaderboardRepository.Update(teamMemberToUpdate);
                if (teamMemberResponse.IsError)
                {
                    _logger.LogError("Database responded with status: {status}", teamMemberResponse.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in StarpointMessageService.AddStarpoints");
                return false;
            }

            return true;
        }

        private async Task<TeamMemberLeaderboardEntity?> GetCorrectTeamMemberLeaderboardEntity(
            StarpointLeaderboardModel starpointLeaderboardModel)
        {
            // Gets all of the TeamMemberEntities from the database that has the same TeamMemberId as the starpointLeaderboardModel
            var teamMemberEntities = await _teamMemberLeaderboardRepository.Get(tm =>
                tm.RowKey == starpointLeaderboardModel.TeamMemberId.ToString());

            foreach (var member in teamMemberEntities.Where(
                         tm => tm.CreatedDate < starpointLeaderboardModel.CreatedDate))
            {
                var team = await _teamLeaderboardRepository.Get(t => t.RowKey == member.PartitionKey);
                if (team.FirstOrDefault().EndDate > starpointLeaderboardModel.CreatedDate)
                {
                    return member;
                }
            }

            return null;
        }
        #endregion

        #region Starpoint backup management 
        private async Task<StarpointEntity?> GetCorrectStarpointEntity(
           StarpointLeaderboardModel starpointLeaderboardModel)
        {
            // Gets all of the StarpointEntities from the database that has the same TeamMemberId as the starpointLeaderboardModel
            var starpointEntities = await _starpointLeaderboardRepository.Get(tm =>
                tm.RowKey == starpointLeaderboardModel.StarpointId.ToString() );

            foreach (var starpointEntity in starpointEntities)
            {               
                if (starpointEntity.PartitionKey == starpointLeaderboardModel.TeamMemberId.ToString())
                {
                    return starpointEntity;
                }
            }

            return null;
        }

        public async Task<bool> AddStarpointsToBackup(StarpointLeaderboardModel starpointLeaderboardModel)
        {
            var starPointEntityToUpdate =
                await GetCorrectStarpointEntity(starpointLeaderboardModel);

            if (starPointEntityToUpdate == null)
            {
                // Tries to add the starpoints to the starpoint backup table by using the Add method on the IGenericRepository<StarpointLeaderboardEntity>
                try
                {
                    starPointEntityToUpdate = new StarpointEntity(starpointLeaderboardModel.TeamMemberId,
                                                    starpointLeaderboardModel.StarpointId, starpointLeaderboardModel.Starpoints, starpointLeaderboardModel.CreatedDate);
                    
                    var starPointResponse = await _starpointLeaderboardRepository.Add(starPointEntityToUpdate);
                    if (starPointResponse.IsError)
                    {
                        _logger.LogError("Database responded with status: {status}", starPointResponse.Status);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception caught in StarpointMessageService.AddStarpointsToBackup");
                    return false;
                }
                return true;
            }
            else {
            // Tries to update the starpoints to the starpoint backup table by using the Update method on the IGenericRepository<StarpointLeaderboardEntity>
                    try
                    {
                        starPointEntityToUpdate.Starpoints += starpointLeaderboardModel.Starpoints;
                        var starPointResponse = await _starpointLeaderboardRepository.Update(starPointEntityToUpdate);
                        if (starPointResponse.IsError)
                        {
                            _logger.LogError("Database responded with status: {status}", starPointResponse.Status);
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception caught in StarpointMessageService.AddStarpointsToBackup");
                        return false;
                    }
                return true;
            }

           
        }

        public async Task<bool> RemoveStarpointsFromBackup(StarpointLeaderboardModel starpointLeaderboardModel)
        {
            var starPointBackupToUpdate =
                await GetCorrectStarpointEntity(starpointLeaderboardModel);
            if (starPointBackupToUpdate == null)
            {
                _logger.LogError(
                    "No Entity with RowKey: {id} are currently active", starpointLeaderboardModel.TeamMemberId);
                return false;
            }

            // Tries to remove the starpoints from the StarPoint backup by using the Update method on the IGenericRepository<StarpointEntity>
            try
            {
                starPointBackupToUpdate.Starpoints -= starpointLeaderboardModel.Starpoints;
                var starPointBackupResponse = await _starpointLeaderboardRepository.Update(starPointBackupToUpdate);
                if (starPointBackupResponse.IsError)
                {
                    _logger.LogError("Database responded with status: {status}", starPointBackupResponse.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught in StarpointMessageService.RemoveStarpointsFromBackup");
                return false;
            }

            return true;
        }
        #endregion
    }
}