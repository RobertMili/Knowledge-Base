using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BoostApp.ClassLibrary;
using FakeItEasy;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services;
using LeaderboardApi.Services.Interfaces;
using Xunit;

namespace LeaderboardApi.Tests
{
    public class CompetitionMessageServiceTests
    {
        [Fact]
        public async Task HandleMessageAsync_ReturnsTrue_WhenRequestIsPut()
        {
            // Arrange
            var teamRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            var message = new LeaderboardMessageModel
            {
                Request = RequestEnum.PUT,
                CompetitionId = 1,
                EndDate = DateTime.Now.AddDays(1)
            };
            var competitionMessageService = new CompetitionMessageService(teamRepository);

            // Act
            var result = await competitionMessageService.HandleMessageAsync(message);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HandleMessageAsync_ReturnsFalse_WhenRequestIsNotPut()
        {
            // Arrange
            var teamRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            var message = new LeaderboardMessageModel
            {
                Request = RequestEnum.POST,
                CompetitionId = 1,
                EndDate = DateTime.Now.AddDays(1)
            };
            var competitionMessageService = new CompetitionMessageService(teamRepository);

            // Act
            var result = await competitionMessageService.HandleMessageAsync(message);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateEndDateForTeamsAsync_UpdatesEndDateForAllTeamsInCompetition()
        {
            // Arrange
            var teamRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            var competitionId = 1;
            var newEndDate = DateTime.Now.AddDays(1);
            var teams = new List<TeamLeaderboardEntity>
            {
                new TeamLeaderboardEntity { PartitionKey = competitionId.ToString(), EndDate = DateTime.Now },
                new TeamLeaderboardEntity { PartitionKey = competitionId.ToString(), EndDate = DateTime.Now }
            };

            A.CallTo(() => teamRepository.Get(
                A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>.Ignored,
                A<string>.Ignored)).Returns(teams);

            var competitionMessageService = new CompetitionMessageService(teamRepository);

            // Act
            await competitionMessageService.UpdateEndDateForTeamsAsync(competitionId, newEndDate);

            // Assert
            foreach (var team in teams)
            {
                Assert.Equal(newEndDate, team.EndDate);
                A.CallTo(() => teamRepository.Update(team)).MustHaveHappenedOnceExactly();
            }
        }
    }
}
