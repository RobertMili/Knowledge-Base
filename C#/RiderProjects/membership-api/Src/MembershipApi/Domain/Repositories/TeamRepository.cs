using MembershipApi.Domain.Repositories.Entities;
using MembershipApi.Infrastructure.Azure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MembershipApi.Domain.Repositories
{
    public class TeamRepository : AzureTableStorageBase<TeamEntity>
    {
        public TeamRepository(
            IConfiguration configuration,
            IDistributedCache distributedCache,
            ILogger<TeamRepository> logger)
            : base(configuration, logger)
        {

        }
    }
}