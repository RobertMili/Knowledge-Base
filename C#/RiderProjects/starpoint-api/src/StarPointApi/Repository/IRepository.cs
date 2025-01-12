using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using StarPointApi.DTOs;
using StarPointApi.Repository.Models;

namespace StarPointApi.Repository
{
    public interface IRepository
    {
        Task<IEnumerable<StarPointEntity>> GetActivitiesByUserID(string userID, DateTime from, DateTime to,
            string source);

        /// <summary>
        ///     Returns the rowKey for the inserted Entity
        /// </summary>
        /// <param name="starPointEntity"></param>
        /// <returns></returns>
        Task<TableResult> AddOrUpdateActivity(StarPointEntity starPointEntity);

        Task<TableResult> RemoveActivity(string rowKey, string userId);
        public Task<TableResult> GetActivityAsync(string userId, string rowKey);
    }
}