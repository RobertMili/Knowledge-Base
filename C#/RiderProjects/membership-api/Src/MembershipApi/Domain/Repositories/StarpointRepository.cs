using MembershipAPI.Domain.Interfaces;
using MembershipAPI.Domain.Repositories.Entities;
using MembershipAPI.DTO;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Domain.Repositories
{
    public class StarpointRepository : IStarpointsRepository
    {
        private readonly CloudTable _userTable;

        public StarpointRepository(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            _userTable = client.GetTableReference("Users");
            _userTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        public async Task<UserEntity> AddOrEditUser(string userID, string competitionID, string teamID)
        {
            try
            {
                TableQuery<UserEntity> query = new TableQuery<UserEntity>()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userID));
                var user = await _userTable.ExecuteQuerySegmentedAsync(query, null);
                if (user.Results.Count != 0)
                {

                    var updateUser = user.Where(u => u.RowKey == userID).FirstOrDefault();

                    updateUser.TeamID = teamID;
                    updateUser.CompetitionID = competitionID;

                    var updateQuery = TableOperation.Replace(updateUser);
                    await _userTable.ExecuteAsync(updateQuery);
                }

                UserEntity addUser = new UserEntity
                {
                    RowKey = userID,
                    PartitionKey = userID,
                    CompetitionID = competitionID,
                    TeamID = teamID,
                    TotalStarpoints = 0
                };

                var addUserQuery = TableOperation.Insert(addUser);

                await _userTable.ExecuteAsync(addUserQuery);

                return addUser;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> GetTotalStarpointsForTeamByTeamID(string teamID)
        {
            
            TableQuery<UserEntity> query = new TableQuery<UserEntity>()
                    .Where(TableQuery.GenerateFilterCondition("TeamID", QueryComparisons.Equal, teamID));

            var data = await _userTable.ExecuteQuerySegmentedAsync(query, null);

            if (!data.Any())
            {
                return 0;
            }
            var teamPoints = data.Results.Select(p => p.TotalStarpoints).Sum();

            return teamPoints;
        }

        public async Task<int> GetTotalStarpointsForUserByUserID(string userID)
        {
            TableQuery<UserEntity> query = new TableQuery<UserEntity>()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userID));

            var data = await _userTable.ExecuteQuerySegmentedAsync(query, null);
            if (!data.Any())
            {
                return 0;
            }
            var userPoints = data.Results.Select(u => u.TotalStarpoints).FirstOrDefault();

            return userPoints;
        }

        public async Task<IList<UserEntity>> GetUserEntityByCompetitionID(string competitionID)
        {
            TableQuery<UserEntity> query = new TableQuery<UserEntity>()
                    .Where(TableQuery.GenerateFilterCondition("CompetitionID", QueryComparisons.Equal, competitionID));
            
            var data = await _userTable.ExecuteQuerySegmentedAsync(query, null);
            if (!data.Any())
            {
                return null;
            }
            return data.Select(u => u).ToList();
        }

        public async Task<IList<UserEntity>> GetAllUserEntities()
        {
            TableQuery<UserEntity> query = new TableQuery<UserEntity>();

            var users = await _userTable.ExecuteQuerySegmentedAsync(query, null);
            
            if (!users.Any())
            {
                return null;
            }




            return users.Select(u => u).ToList();
        }
    }
}
