using MembershipApi.Domain.Repositories.Entities;
using MembershipApi.Infrastructure.Azure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MembershipApi.Domain.Repositories
{
    public class TeamMemberRepository : AzureTableStorageBase<TeamMemberEntity>
    {
        public TeamMemberRepository(
            IConfiguration configuration,
            IDistributedCache distributedCache,
            ILogger<TeamMemberRepository> logger)
            : base(configuration, logger)
        {

        }
    }
}