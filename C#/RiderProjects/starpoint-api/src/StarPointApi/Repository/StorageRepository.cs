using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using StarPointApi.Repository.Models;
using StarPointApi.DTOs;

namespace StarPointApi.Repository
{
    public class StorageRepository : IRepository
    {
        private readonly CloudTable _activityTable;

        public StorageRepository(string connectionstring)
        {
            // Parsing the connection string to get the CloudStorageAccount object
            var account = CloudStorageAccount.Parse(connectionstring);
            // Creating a CloudTableClient object using the CloudStorageAccount object
            var client = account.CreateCloudTableClient();
            // Getting a reference to the CloudTable object named "BoostAppActivityTable"
            _activityTable = client.GetTableReference("Starpoint");
            // Creating the table if it does not exist
            _activityTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        public async Task<TableResult> AddOrUpdateActivity(StarPointEntity starPointEntity)
        {
            // Checking if the activity already exists in the table
            if (ActivityExist(starPointEntity))
            {
                // Setting the ETag of the activity to "*" to indicate that it should be replaced regardless of its current state
                starPointEntity.ETag = "*";
                // Creating a TableOperation object to replace the existing activity with the updated one
                var update = TableOperation.Replace(starPointEntity);
                // Executing the update operation on the table and returning the TableResult object
                return await _activityTable.ExecuteAsync(update);
            }

            // Creating a TableOperation object to insert the new activity into the table
            var insertOperation = TableOperation.Insert(starPointEntity);
            // Executing the insert operation on the table and returning the TableResult object
            return await _activityTable.ExecuteAsync(insertOperation);
        }

        public async Task<IEnumerable<StarPointEntity>> GetActivitiesByUserID(string userID, DateTime from, DateTime to,
            string source)
        {
            // Converting the from and to DateTime objects to UTC
            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            // Creating a filter condition for the PartitionKey property of the StarPointEntity object that matches the userID argument
            var userFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userID);

            // Creating a filter condition for the StartDate property of the StarPointEntity object that is greater than or equal to the fromUtc DateTime object
            var fromFilter = TableQuery.GenerateFilterConditionForDate(nameof(StarPointEntity.CreatedDate),
                QueryComparisons.GreaterThanOrEqual, new DateTimeOffset(fromUtc));

            // Creating a filter condition for the EndDate property of the StarPointEntity object that is less than or equal to the toUtc DateTime object
            var toFilter = TableQuery.GenerateFilterConditionForDate(nameof(StarPointEntity.CreatedDate),
                QueryComparisons.LessThanOrEqual, new DateTimeOffset(toUtc));

            // Combine the filter conditions using the AND operator.
            string Combine(string filterA, string filterB) =>
                TableQuery.CombineFilters(filterA, TableOperators.And, filterB);

            // Aggregate the list of filter conditions using the Combine method.
            string CombineFilters(List<string> filters) => filters.Aggregate(Combine);

            // creates a list of filters to apply to the query. These filters include the user ID, the start and end dates of the query.
            var filterList = new List<string> {userFilter, fromFilter, toFilter};


            if (!String.IsNullOrWhiteSpace(source))
            {
                filterList.Add(TableQuery.GenerateFilterCondition("Source", QueryComparisons.Equal, source));
            }

            var combinedFilter = CombineFilters(filterList);

            var query = new TableQuery<StarPointEntity>().Where(combinedFilter);

            var result = new List<StarPointEntity>();
            TableContinuationToken token = null;
            // Continuously execute the query until there are no more results to return
            do
            {
                var segment = await _activityTable.ExecuteQuerySegmentedAsync(query, token);
                result.AddRange(segment);
                token = segment.ContinuationToken;
            } while (token != null);

            return result;
        }

        public async Task<TableResult> RemoveActivity(string rowKey, string userId)
        {
            // Creates a TableOperation object to retrieve the activity with the specified row key and user ID.
            var retrive = TableOperation.Retrieve<StarPointEntity>(userId, rowKey);
            // Executes the retrieval operation and returns the result.
            var result = await _activityTable.ExecuteAsync(retrive);
            // Retrieves the activity entity from the result.
            var entity = (StarPointEntity) result.Result;
            if (entity == null)
                return result;
            // Creates a TableOperation object to delete the activity entity.
            var delete = TableOperation.Delete(entity);
            var deleteResult = await _activityTable.ExecuteAsync(delete);

            return deleteResult;
        }

        public async Task<TableResult> GetActivityAsync(string userId, string rowKey)
        {
            var query = TableOperation.Retrieve<StarPointEntity>(userId, rowKey);
            var result = await _activityTable.ExecuteAsync(query);
            return result;
        }

        private bool ActivityExist(StarPointEntity starPointEntity)
        {
            var query = TableOperation.Retrieve<StarPointEntity>(starPointEntity.UserID, starPointEntity.RowKey);
            var result = _activityTable.Execute(query);
            return result.Result != null;
        }
    }
}