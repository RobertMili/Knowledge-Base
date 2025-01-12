using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Collector.Common.RestClient;
using Microsoft.Extensions.Logging;
using StarPointApi.Apis.Steps;
using StarPointApi.DTOs;
using StarPointApi.Services.CronJobs.Config;
using StarPointApi.Services.StarPointService;
using StarPointApi.Shared;

namespace StarPointApi.Services.CronJobs
{
    public class GetStepsJob : CronJobBase
    {
        private readonly ILogger<GetStepsJob> _logger;
        private readonly IRestApiClient _restClient;
        private readonly IStarPointService _starPointService;

        public GetStepsJob(ICronJobConfig<GetStepsJob> config, ILogger<GetStepsJob> logger, IRestApiClient restClient,
            IStarPointService starPointService)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _restClient = restClient;
            _starPointService = starPointService;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} {nameof(GetStepsJob)} started");

            var yesterday = DateTime.UtcNow.AddDays(-1).Date;
            var endOfYesterday =
                new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 23, 59, 59, DateTimeKind.Utc);

            var rawActivities =
                await _restClient.CallAsync(new StepApi.GetStarPointsRequest(yesterday, endOfYesterday));

            var activities = rawActivities.Select(x => new PutStarPointsDTO
            {
                StarPoints = x.StarPoint.StarPoints,
                UserId = x.UserId.ToString(),
                Source = x.StarPoint.Source,
                DatabaseId = Guid.NewGuid().ToString(),
            }).ToList();

            foreach (var postActivityDto in activities)
                await _starPointService.PostOrEditUserActivity(postActivityDto);

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} {nameof(GetStepsJob)} completed");

            await Task.CompletedTask;
        }
    }
}