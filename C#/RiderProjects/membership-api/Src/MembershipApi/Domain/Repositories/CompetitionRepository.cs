using MembershipApi.Domain.Repositories.Entities;
using MembershipApi.Infrastructure.Azure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MembershipApi.Domain.Repositories
{
    public class CompetitionRepository : AzureTableStorageBase<CompetitionEntity>
    
    // : AzureBlobStorageRepositoryBase<ContestCatalog>, IRepositoryOfType<ContestCatalog>

    {
        // public ContestRepository(
        //     IConfiguration configuration, 
        //     CloudBlobClient cloudBlobClient,
        //     IDistributedCache distributedCache,
        //     ILogger<ContestRepository> logger)
        //     : base(configuration, cloudBlobClient, distributedCache, logger)
        // {

        // }

        public CompetitionRepository(
            IConfiguration configuration,
            IDistributedCache distributedCache,
            ILogger<CompetitionRepository> logger)
            : base(configuration, logger)
        {

        }

        // protected override string GetEntityKey(ContestCatalog entity)
        //     => entity.GetType().Name.ToLower();
    }
}