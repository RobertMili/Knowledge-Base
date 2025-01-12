using Azure;
using Azure.Data.Tables;
using System.Linq.Expressions;

namespace LeaderboardApi.DAL.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class, ITableEntity
    {
        public Task<IEnumerable<TEntity>> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        public Task<TEntity> GetById(string partitionKey, string rowKey);
        public Task<Response> Add(TEntity entity);
        public Task<Response> Update(TEntity entity);
        public Task<Response> Delete(string partitionKey, string rowKey);
    }
}