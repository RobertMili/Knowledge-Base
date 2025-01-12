using System;
using System.Linq.Expressions;
using Azure;
using BoostApp.ClassLibrary;
using FakeItEasy;
using FluentAssertions;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services;
using LeaderboardApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LeaderboardApi.Tests
{
    public class TeamMemberMessageServiceTests
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamLeaderboardRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _teamMemberLeaderboardRepository;
        private readonly ILogger<TeamMemberMessageService> _logger;
        private readonly IHttpClientService _httpClientService;
        private readonly IExternalApiService _externalApiService;

        public TeamMemberMessageServiceTests()
        {
            _teamLeaderboardRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            _teamMemberLeaderboardRepository = A.Fake<IGenericRepository<TeamMemberLeaderboardEntity>>();
            _logger = A.Fake<ILogger<TeamMemberMessageService>>();
            _httpClientService = A.Fake<IHttpClientService>();
            _externalApiService = A.Fake<IExternalApiService>();
        }

        [Fact]
        async Task TeamMemberMessageService_AddTeamMember_ReturnsTrue()
        {
            // Arrange
            var sut = new TeamMemberMessageService(_teamMemberLeaderboardRepository, _teamLeaderboardRepository, _externalApiService, _logger);
            var fakeResponse = A.Fake<Response>();

            var fakeMessageModel = new LeaderboardMessageModel
            {
                TeamId = Guid.NewGuid(),
                TeamMemberId = Guid.NewGuid(),
                TeamMemberName = "FakeName",
            };

            var fakeTeamMemberEntity = new TeamMemberLeaderboardEntity
            {
                PartitionKey = fakeMessageModel.TeamId.ToString(),
                RowKey = fakeMessageModel.TeamMemberId.ToString(),
                Name = fakeMessageModel.TeamMemberName,
                CreatedDate = DateTime.UtcNow
            };

            var fakeList = new List<TeamLeaderboardEntity>
                {
                    new TeamLeaderboardEntity
                    {
                        RowKey = fakeTeamMemberEntity.PartitionKey,
                        PartitionKey = "1",
                        Name = "fakeTeamName",
                        ImageUrl = "fakeImgUrl",
                        CreatedDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(1)
                    }
                };

            A.CallTo(() => _teamLeaderboardRepository.Get(A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>
                        .Ignored, A<string>.Ignored))
                .Returns(fakeList);

            var competitionId = fakeList.FirstOrDefault()?.PartitionKey;

            var fakeCompetition = new CompetitionResponseModel
            {
                Id = Convert.ToInt32(competitionId),
                Description = "fakeDescription",
                Name = "fakeName",
                Image = "fakeImage",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            A.CallTo(() => _externalApiService.GetCompetition(Convert.ToInt32(competitionId))).Returns(fakeCompetition);

            if (fakeCompetition.EndDate >= DateTime.UtcNow)
            {
                var startDate = fakeCompetition?.StartDate;

                var fakeStarpoints = new StarpointResponseModel
                {
                    UserId = fakeTeamMemberEntity.PartitionKey,
                    TotalStarPoints = 1000,
                    From = DateTime.UtcNow,
                    To = DateTime.UtcNow.AddDays(1)
                };

                A.CallTo(() => _externalApiService.GetExistingStarpointsAsync(startDate.Value, fakeMessageModel.TeamMemberId.ToString()))
                    .Returns(fakeStarpoints.TotalStarPoints);

                if (fakeStarpoints.TotalStarPoints != null)
                {
                    fakeTeamMemberEntity.Starpoints = fakeStarpoints.TotalStarPoints;
                }
            }

            A.CallTo(() => _teamMemberLeaderboardRepository.Add(fakeTeamMemberEntity))
                .Returns(fakeResponse);

            // Act
            var result = await sut.AddTeamMember(fakeMessageModel);

            // Assert
            result.Should().BeTrue();
        }


        [Fact]
        public async Task TeamMemberMessageService_UpdateTeamMember_ReturnsTrue()
        {
            // Arrange
            var sut = new TeamMemberMessageService(_teamMemberLeaderboardRepository, _logger);
            var fakeResponse = A.Fake<Response>();
            var fakeEntity = new TeamMemberLeaderboardEntity
            {
                Name = "FakeName",
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = Guid.NewGuid().ToString()
            };
            var fakeMessageModel = new LeaderboardMessageModel
            {
                TeamMemberName = "NewFakeName",
                TeamId = Guid.NewGuid(),
                TeamMemberId = Guid.NewGuid()
            };

            fakeEntity.Name = fakeMessageModel.TeamMemberName;
            fakeEntity.PartitionKey = fakeMessageModel.TeamId.ToString();
            fakeEntity.RowKey = fakeMessageModel.TeamMemberId.ToString();

            A.CallTo(() => _teamMemberLeaderboardRepository.Update(fakeEntity)).Returns(fakeResponse);

            // Act
            var result = await sut.UpdateTeamMember(fakeMessageModel);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task TeamMemberMessageService_DeleteTeamMember_ReturnsTrue()
        {
            // Arrange
            var sut = new TeamMemberMessageService(_teamMemberLeaderboardRepository, _logger);
            var fakeResponse = A.Fake<Response>();
            var fakeTeamId = Guid.NewGuid();
            var fakeTeamMemberId = Guid.NewGuid();

            A.CallTo(() => _teamMemberLeaderboardRepository.Delete(fakeTeamId.ToString(), fakeTeamMemberId.ToString()))
                .Returns(fakeResponse);

            // Act
            var result = await sut.DeleteTeamMember(fakeTeamId, fakeTeamMemberId);

            // Assert
            result.Should().BeTrue();
        }

    }
}
