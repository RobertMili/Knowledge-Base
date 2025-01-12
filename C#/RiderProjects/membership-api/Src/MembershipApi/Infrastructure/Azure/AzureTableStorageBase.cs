using System.Collections.Generic;
using System.Threading.Tasks;
using MembershipApi.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MembershipApi.Infrastructure.Azure
{
    public abstract class AzureTableStorageBase<TEntity>
        : ITableOfType<TEntity> where TEntity : ITableEntity, new()
    {
        private IConfiguration configuration;

        private readonly ILogger<AzureTableStorageBase<TEntity>> _logger;

        private readonly CloudTableClient _tableClient;

        private readonly CloudTable _table;

        protected AzureTableStorageBase(IConfiguration configuration, ILogger<AzureTableStorageBase<TEntity>> logger)
        {
            this.configuration = configuration;
            _logger = logger;

            var connectionString = configuration["Azure:StorageConnectionString"];
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = cloudStorageAccount.CreateCloudTableClient();
            var tableName = typeof(TEntity).Name.ToLower();
            _table = _tableClient.GetTableReference(tableName);
            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<TEntity> SaveAsync(TEntity entity)
        {
            _logger.LogInformation("SaveAsync {@entity}", entity);
            var operation = TableOperation.InsertOrReplace(entity);
            var result = await _table.ExecuteAsync(operation);
            return (TEntity) result.Result;
        }

        public async Task<IList<TEntity>> ReadPartitionAsync(string partitionKey)
        {
            _logger.LogInformation("ReadPartitionAsync {partitionKey}", partitionKey);
            TableQuery<TEntity> query = new TableQuery<TEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var data = await _table.ExecuteQuerySegmentedAsync(query, null);
            return data.Results;
        }

        public async Task<IList<TEntity>> ReadTeamIDAsync(string teamID)
        {
            _logger.LogInformation("ReadPartitionAsync {teamId}", teamID);
            TableQuery<TEntity> query = new TableQuery<TEntity>()
                    .Where(TableQuery.GenerateFilterCondition("TeamId", QueryComparisons.Equal, teamID));

            var data = await _table.ExecuteQuerySegmentedAsync(query, null);
            return data.Results;
        }

        public async Task<TEntity> ReadAsync(string partitionKey, string rowKey)
        {
            _logger.LogInformation("ReadAsync {partitionKey}/{rowKey}", partitionKey, rowKey);
            TableOperation retrieve = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);
            TableResult result = await _table.ExecuteAsync(retrieve);
            return (TEntity) result.Result;
        }

        public async Task<TEntity> RemoveAsync(string partitionKey, string rowKey)
        {
            var entity = await ReadAsync(partitionKey, rowKey);
            _logger.LogInformation("RemoveAsync found {@entity}", entity);

            TableResult result = await _table.ExecuteAsync(TableOperation.Delete(entity));
            _logger.LogDebug("Remove result {@result}", result);
            return (TEntity) result.Result;
        }
    }
}