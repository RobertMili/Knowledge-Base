using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MembershipApi.Domain.Interfaces
{
    public interface ITableOfType<T> where T : ITableEntity, new()
    {
        Task<T> SaveAsync(T entity);

        Task<IList<T>> ReadPartitionAsync(string partitionKey);

        Task<T> ReadAsync(string partitionKey, string rowKey);

        Task<T> RemoveAsync(string partitionKey, string rowKey);

        Task<IList<T>> ReadTeamIDAsync(string teamId);
    }
}