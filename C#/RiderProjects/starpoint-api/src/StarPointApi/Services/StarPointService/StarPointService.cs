using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoostApp.ClassLibrary;
using BoostApp.Shared.Messaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StarPointApi.DTOs;
using StarPointApi.Repository;
using StarPointApi.Repository.Models;
using StarPointApi.Services.MessageService;

namespace StarPointApi.Services.StarPointService
{
    public partial class StarPointService : IStarPointService
    {
        private readonly IRepository _repository;
        private readonly IMessageService _messageService;

        public StarPointService(IRepository repository, IMessageService messageService)
        {
            _repository = repository;
            _messageService = messageService;
        }

        public async Task<UserHistoryDTO> GetStarPointHistoryByUserIdAsync(string userId, DateTime startDate,
            DateTime endDate, string source)
        {
            var activities = await _repository.GetActivitiesByUserID(userId, startDate, endDate, source);
            return activities.Any() ? GetUserActivityFromList(activities) : new UserHistoryDTO();
        }

        public async Task<UserTotalDTO> GetTotalStarPointsByUserIdAsync(
            string userId, DateTime startDate, DateTime endDate, string source)
        {
            var userActivities = await _repository.GetActivitiesByUserID(userId, startDate, endDate, source);
            var totalPoints = userActivities.Sum(act => act.StarPoints);

            if (!userActivities.Any())
                return null;

            return new UserTotalDTO
            {
                UserID = userActivities.FirstOrDefault().PartitionKey,
                From = startDate,
                To = endDate,
                TotalStarPoints = totalPoints
            };
        }

        public async Task<AddUserResponseDTO> PostOrEditUserActivity(PutStarPointsDTO putStarPoints)
        {
            var addStarpoints = new StarPointEntity(
                putStarPoints.UserId,
                putStarPoints.Source,
                DateTime.Now,
                putStarPoints.StarPoints,
                putStarPoints.SourceID,
                putStarPoints.DatabaseId);


            var tableResult = await _repository.GetActivityAsync(addStarpoints.UserID, addStarpoints.RowKey);
            if (IsSuccessStatusCode(tableResult.HttpStatusCode))
            {
                addStarpoints.CreatedDate = (tableResult.Result as StarPointEntity).CreatedDate;
                var putResult = await _repository.AddOrUpdateActivity(addStarpoints);
                if (IsSuccessStatusCode(putResult.HttpStatusCode))
                {
                    var newStarpoints = addStarpoints.StarPoints;
                    var oldStarpoints = (tableResult.Result as StarPointEntity).StarPoints;
                    if (newStarpoints != oldStarpoints)
                    {
                        await _messageService.PublishPutMessageAsync(addStarpoints, oldStarpoints);
                    }
                }
            }
            else
            {
                var postResult = await _repository.AddOrUpdateActivity(addStarpoints);

                if (IsSuccessStatusCode(postResult.HttpStatusCode))
                {
                    await _messageService.PublishPostMessageAsync(addStarpoints);
                }
            }

            return new AddUserResponseDTO {DatabaseID = addStarpoints.RowKey};
        }

        public async Task<bool> DeleteUserActivity(string databaseID, string userId)
        {
            var result = await _repository.RemoveActivity(databaseID, userId);
            if (IsSuccessStatusCode(result.HttpStatusCode))
            {
                await _messageService.PublishDeleteMessageAsync((result.Result as StarPointEntity));
                return true;
            }

            return false;
        }

        public async Task<TimeSeriesDTO> GetTimeSeriesByUserIdAsync(string userId, DateTime startDate, DateTime endDate,
            TimeFrame timeFrame, string source)
        {
            var rawData = await _repository.GetActivitiesByUserID(userId, startDate, endDate, source);
            return TimeSeriesConverter.Convert(userId, rawData.ToArray(), timeFrame, startDate, endDate);
        }

        private static UserHistoryDTO GetUserActivityFromList(IEnumerable<StarPointEntity> activities)
        {
            return new UserHistoryDTO
            {
                UserID = activities.FirstOrDefault().PartitionKey,
                Activities = activities.Select(act => new SourceDTO
                {
                    Source = act.Source,
                    StarPoints = act.StarPoints,
                    CreatedDate = act.CreatedDate,
                    DatabaseId = act.RowKey
                }).ToList()
            };
        }

        public async Task<AddUserResponseDTO> PostStarpoints(PostStarpointsDTO starpointsDTO)
        {
            var addStarpoints = new StarPointEntity
            (
                starpointsDTO.UserId,
                starpointsDTO.Source,
                DateTime.Now,
                starpointsDTO.Starpoints,
                starpointsDTO.SourceID
            );

            var request = await _repository.AddOrUpdateActivity(addStarpoints);

            if (IsSuccessStatusCode(request.HttpStatusCode))
            {
                await _messageService.PublishPostMessageAsync(addStarpoints);
            }

            return new AddUserResponseDTO {DatabaseID = (request.Result as StarPointEntity).RowKey};
        }

        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}