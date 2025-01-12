using System;
using System.Linq;
using StarPointApi.DTOs;
using StarPointApi.Repository;
using StarPointApi.Services.MessageService;
using StarPointApi.Services.StarPointService;
using StarPointApi.Shared;
using StarPointApiTests.Helpers;
using Xunit;

namespace StarPointApiTests
{
    public class StarPointServiceTest
    {
        [Fact]
        public async void AddActivity()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var databaseId = await service.PostOrEditUserActivity(ModelBuilder.GetPutStarpointsDto(x => { }));

            Assert.NotNull(databaseId);
            Assert.NotEqual("", databaseId.DatabaseID);
        }

        [Fact]
        public async void AddAndRemoveActivity()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var databaseId =
                await service.PostOrEditUserActivity(ModelBuilder.GetPutStarpointsDto(x => x.UserId = "Id1"));

            var result = await service.DeleteUserActivity(databaseId.DatabaseID, "Id1");
            Assert.True(result);
        }

        [Fact]
        public async void GetTotalScoreByUserId()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            await service.PostOrEditUserActivity(ModelBuilder.GetPutStarpointsDto(x =>
            {
                x.StarPoints = 1337;
                x.UserId = "Id1";
            }));

            var totalScore =
                await service.GetTotalStarPointsByUserIdAsync("Id1", DateTime.Now.AddDays(-2),
                    DateTime.Now.AddDays(1), null);
            Assert.NotNull(totalScore);
            Assert.Equal(1337, totalScore.TotalStarPoints);
        }

        [Fact]
        public async void UpdateExistingActivity()
        {
            var service = new StarPointService(new MockRepository(), new MockMessageService());
            var databaseId = await service.PostOrEditUserActivity(ModelBuilder.GetPutStarpointsDto(x =>
            {
                x.UserId = "Id1";
                x.StarPoints = 1336;
            }));
            var updateActivity = await service.PostOrEditUserActivity(ModelBuilder.GetPutStarpointsDto((x =>
            {
                x.UserId = "Id1";
                x.DatabaseId = databaseId.DatabaseID;
                x.StarPoints = 1337;
            })));

            var totalPoints =
                await service.GetTotalStarPointsByUserIdAsync("Id1", DateTime.Now.AddDays(-2),
                    DateTime.Now.AddDays(2), null);
            Assert.NotNull(databaseId);
            Assert.NotNull(updateActivity);
            Assert.Equal(databaseId.DatabaseID, updateActivity.DatabaseID);
            Assert.Equal(1337, totalPoints.TotalStarPoints);
        }
    }
}