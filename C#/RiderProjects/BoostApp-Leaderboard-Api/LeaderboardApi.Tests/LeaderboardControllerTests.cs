using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using LeaderboardApi.Controllers;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.DTOs;
using LeaderboardApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LeaderboardApi.Tests
{
    public class LeaderboardControllerTests
    {
        [Fact]
        public async Task GetTeamLeaderboard_ReturnsListOfTeams()
        {
            // Arrange
            var expectedCompetitionId = 1;
            var expectedTeams = new List<TeamLeaderboardDto>
            {
                new TeamLeaderboardDto { TeamId = Guid.NewGuid(), CompetitionId = expectedCompetitionId, Name = "Team A", ImageUrl = "https://image.com/teamA.jpg", TotalStarpoints = 10 },
                new TeamLeaderboardDto { TeamId = Guid.NewGuid(), CompetitionId = expectedCompetitionId, Name = "Team B", ImageUrl = "https://image.com/teamB.jpg", TotalStarpoints = 20 }
            };

            var teamRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            var teamMemberRepository = A.Fake<IGenericRepository<TeamMemberLeaderboardEntity>>();
            A.CallTo(() => teamRepository.Get(
                    A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>.Ignored,
                    A<string>.Ignored))
                .Returns(expectedTeams.Select(t => new TeamLeaderboardEntity
                {
                    PartitionKey = t.CompetitionId.ToString(),
                    RowKey = t.TeamId.ToString(),
                    Name = t.Name,
                    ImageUrl = t.ImageUrl,
                    Timestamp = DateTime.UtcNow
                }));

            var teamMemberTasks = expectedTeams.Select(async t =>
            {
                var teamMemberEntities = new List<TeamMemberLeaderboardEntity>
                {
                    new TeamMemberLeaderboardEntity { PartitionKey = t.TeamId.ToString(), RowKey = Guid.NewGuid().ToString(), Name = "John Doe", Starpoints = 5 },
                    new TeamMemberLeaderboardEntity { PartitionKey = t.TeamId.ToString(), RowKey = Guid.NewGuid().ToString(), Name = "Jane Doe", Starpoints = 5 }
                };

                A.CallTo(() => teamMemberRepository.Get(
                    A<Expression<Func<TeamMemberLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamMemberLeaderboardEntity>, IOrderedQueryable<TeamMemberLeaderboardEntity>>>.Ignored, A<string>.Ignored)).Returns(teamMemberEntities);


                var totalPoints = teamMemberEntities.Sum(tm => tm.Starpoints);
                return new TeamLeaderboardDto
                {
                    TeamId = t.TeamId,
                    CompetitionId = t.CompetitionId,
                    Name = t.Name,
                    ImageUrl = t.ImageUrl,
                    TotalStarpoints = totalPoints
                };
            });

            var expectedTeamLeaderboard = await Task.WhenAll(teamMemberTasks);

            var controller = new LeaderboardController(teamRepository, teamMemberRepository);

            // Act
            var result = await controller.GetTeamLeaderboard(expectedCompetitionId);

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<TeamLeaderboardDto>>>();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.Value.Should().BeEquivalentTo(expectedTeamLeaderboard.OrderByDescending(t => t.TotalStarpoints));
        }
    }
}
