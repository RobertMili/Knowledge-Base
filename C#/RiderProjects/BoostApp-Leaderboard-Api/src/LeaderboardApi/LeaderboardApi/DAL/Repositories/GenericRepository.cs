using Azure;
using Azure.Data.Tables;
using System.Linq.Expressions;

namespace LeaderboardApi.DAL.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, ITableEntity
    {
        private readonly IConfiguration _configuration;
        private readonly TableClient _tableClient;

        public GenericRepository(IConfiguration configuration, string tableName)
        {
            _configuration = configuration;

            _tableClient = new TableClient(
                new Uri(_configuration["AzureStorageAccount:StorageUri"]),
                tableName,
                new TableSharedKeyCredential(_configuration["AzureStorageAccount:StorageAccountName"],
                    _configuration["AzureStorageAccount:StorageAccountKey"]));
        }

        public async Task<IEnumerable<TEntity>> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            var entityList = new List<TEntity>();
            AsyncPageable<TEntity>? entities;

            if (includeProperties != "")
            {
                entities = _tableClient.QueryAsync(filter,
                    select: includeProperties.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                entities = _tableClient.QueryAsync(filter);
            }

            await foreach (var entity in entities)
            {
                entityList.Add(entity);
            }

            if (orderBy != null)
            {
                return orderBy(entityList.AsQueryable()).ToList();
            }

            return entityList;
        }

        public async Task<TEntity> GetById(string partitionKey, string rowKey)
        {
            return await _tableClient.GetEntityAsync<TEntity>(partitionKey, rowKey);
        }

        public async Task<Response> Add(TEntity entity)
        {
            return await _tableClient.AddEntityAsync(entity);
        }

        public async Task<Response> Update(TEntity entity)
        {
            var entityToReplace = await GetById(entity.PartitionKey, entity.RowKey);
            entity.ETag = entityToReplace.ETag;
            return await _tableClient.UpdateEntityAsync(entity, entity.ETag);
        }

        public async Task<Response> Delete(string partitionKey, string rowKey)
        {
            var entityToDelete = await GetById(partitionKey, rowKey);
            return await _tableClient.DeleteEntityAsync(partitionKey, rowKey, entityToDelete.ETag);
        }

    }
}