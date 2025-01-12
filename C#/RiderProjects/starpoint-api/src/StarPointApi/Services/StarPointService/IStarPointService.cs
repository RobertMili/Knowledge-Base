using System;
using System.Collections;
using System.Threading.Tasks;
using StarPointApi.DTOs;

namespace StarPointApi.Services.StarPointService
{
    public interface IStarPointService
    {
        Task<UserTotalDTO> GetTotalStarPointsByUserIdAsync(string userId, DateTime createdDate, DateTime endDate, string source);

        Task<UserHistoryDTO> GetStarPointHistoryByUserIdAsync(string userId, DateTime createdDate, DateTime endDate, string source);

        /// <summary>
        ///     If the database already contains a activity with the same database ID and User Id, the activity will be updated
        ///     instead of inserted.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>Returns the database ID for the added or updated activity</returns>
        Task<AddUserResponseDTO> PostOrEditUserActivity(PutStarPointsDTO putStarPoints);

        /// <summary>
        ///     Returns true if delete is possible
        /// </summary>
        /// <param name="databaseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteUserActivity(string databaseId, string userId);

        Task<TimeSeriesDTO> GetTimeSeriesByUserIdAsync(string userId, DateTime startDate, DateTime endDate,
            TimeFrame timeFrame, string source);

        Task<AddUserResponseDTO> PostStarpoints(PostStarpointsDTO starpointsDTO);

    }
}