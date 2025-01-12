using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MembershipApi.Domain.Interfaces;
using MembershipApi.Domain.Repositories.Entities;
using Microsoft.Extensions.Logging;
using Sigma.BoostApp.Contracts;
using Sigma.BoostApp.Contracts.GQL.Mutations;

namespace MembershipApi.Services
{
    public class CompetitionService
    {
        private readonly ITableOfType<CompetitionEntity> _contestRepository;
        private readonly ILogger<CompetitionService> _logger;

        public CompetitionService(ITableOfType<CompetitionEntity> competitionRepository, ILogger<CompetitionService> logger)
        {
            _contestRepository = competitionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ContestInfo>> GetAll()
        {
            _logger.LogInformation("GetAll");
            var catalog = await GetContestCatalog();
            return catalog.Select(x => ConvertToContestInfo(x));
        }

        private async Task<ContestInfo> Set(SetContestDetails o)
        {
            _logger.LogDebug("Set {@Contest}", o);
            var entity = new CompetitionEntity(o.ContestId)
            {
                CompetitionId = o.ContestId,
                Description = o.ContestDescription,
                Name = o.ContestName,
                ImageUrl = o.ContestImage,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                CompetitionNumber = int.Parse(o.ContestId)
            };
            
            entity = await _contestRepository.SaveAsync(entity);

            return ConvertToContestInfo(entity);
        }

        internal async Task<ContestInfo> GetById(string contestId)
        {
            var contest = await GetContest(contestId);
            return ConvertToContestInfo(contest);
        }

        private ContestInfo ConvertToContestInfo(CompetitionEntity x)
        {
            if (x == null)
                return null;

            return new ContestInfo
            {
                Id = x.CompetitionNumber.ToString(),
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Image = new ProfileImage
                {
                    Data = x.ImageUrl,
                    DataType = "text/uri-list"
                }
            };
        }

        internal async Task<ContestInfo> UpdateContest(string contestId, SetContestDetails payload)
        {
            payload.ContestId = contestId;

            await Set(payload);

            return await GetById(contestId);
        }

        internal async Task<ContestInfo> AddContest(SetContestDetails payload)
        {
            var competition = await GetAll();
            var competitionId = "";
            var rnd = new Random();
            do 
            {
                _logger.LogInformation("Generate...");
                var r = 10000 + rnd.Next(1, 10000);
                competitionId = r.ToString().PadRight(int.MaxValue.ToString().Length, '0');

            } while (competition.Any(x => x.Id == competitionId));

            payload.ContestId = competitionId;

            await Set(payload);

            return await GetById(competitionId);
        }

        private async Task<CompetitionEntity> GetContest(string competitionId)
        {
            return await _contestRepository.ReadAsync("competition", competitionId);
        }

        private async Task<IEnumerable<CompetitionEntity>> GetContestCatalog()
        {
            return await _contestRepository.ReadPartitionAsync("competition");
        }

        internal async Task<CompetitionEntity> DeleteContest(string competitionId)
        {
            return await _contestRepository.RemoveAsync("competition", competitionId);
        }
    }
}