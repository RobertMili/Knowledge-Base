using FakeItEasy;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services;
using Microsoft.Extensions.Logging;
using Azure;
using BoostApp.ClassLibrary;
using FluentAssertions;
using LeaderboardApi.Services.Interfaces;

namespace LeaderboardApi.Tests
{
    public class TeamMessageServiceTests
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _teamMemberRepository;
        private readonly ILogger<TeamMessageService> _logger;
        private readonly IExternalApiService _externalApiService;

        public TeamMessageServiceTests()
        {
            _teamRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            _teamMemberRepository = A.Fake<IGenericRepository<TeamMemberLeaderboardEntity>>();
            _logger = A.Fake<ILogger<TeamMessageService>>();
            _externalApiService = A.Fake<IExternalApiService>();
        }

        [Fact]
        public async Task TeamMessageService_Post_ReturnsTrue()
        {
            // Arrange
            var sut = new TeamMessageService(_teamRepository, _teamMemberRepository, _logger, _externalApiService);
            var fakeCompetition = A.Dummy<CompetitionResponseModel>();
            var fakeResponse = A.Fake<Response>();
            var fakeModel = new LeaderboardMessageModel
            {
                TeamId = Guid.NewGuid(),
                CompetitionId = 0,
                TeamName = "test",
                TeamImageUrl = ""
            };
            A.CallTo(() => _externalApiService.GetCompetition(A<int>.Ignored)).Returns(fakeCompetition);
            A.CallTo(() => _teamRepository.Add(A<TeamLeaderboardEntity>.Ignored)).Returns(fakeResponse);
            // Act
            var response = await sut.Post(fakeModel);
            // Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async Task TeamMessageService_Delete_ReturnsTrue()
        {
            // Arrange
            var sut = new TeamMessageService(_teamRepository, _teamMemberRepository, _logger, _externalApiService);
            var fakeResponse = A.Fake<Response>();
            A.CallTo(() => _teamRepository.Delete(A<string>.Ignored, A<string>.Ignored)).Returns(fakeResponse);
            A.CallTo(() => _teamMemberRepository.Delete(A<string>.Ignored, A<string>.Ignored)).Returns(fakeResponse);
            // Act
            var response = await sut.Delete(Guid.NewGuid(), 1);
            // Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async Task TeamMessageService_Put_ReturnsTrue()
        {
            // Arrange
            var sut = new TeamMessageService(_teamRepository, _teamMemberRepository, _logger, _externalApiService);
            var fakeResponse = A.Fake<Response>();
            var fakeTeamEntity = A.Dummy<TeamLeaderboardEntity>();
            var fakeModel = new LeaderboardMessageModel
            {
                TeamId = Guid.NewGuid(),
                CompetitionId = 0,
                TeamName = "test",
                TeamImageUrl = ""
            };
            A.CallTo(() => _teamRepository.GetById(A<string>.Ignored, A<string>.Ignored)).Returns(fakeTeamEntity);
            A.CallTo(() => _teamRepository.Update(A<TeamLeaderboardEntity>.Ignored)).Returns(fakeResponse);
            // Act
            var response = await sut.Put(fakeModel);
            // Assert
            response.Should().BeTrue();
        }
    }
}