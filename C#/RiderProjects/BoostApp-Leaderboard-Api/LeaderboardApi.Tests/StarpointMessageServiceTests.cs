using System.Linq.Expressions;
using Azure;
using FakeItEasy;
using FluentAssertions;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.Models;
using LeaderboardApi.Services;
using Microsoft.Extensions.Logging;

namespace LeaderboardApi.Tests
{
    public class StarpointMessageServiceTests
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _teamMemberRepository;
        private readonly IGenericRepository<StarpointEntity> _starpointLeaderboardRepository;
        private readonly ILogger<StarpointMessageService> _logger;

        public StarpointMessageServiceTests()
        {
            _teamRepository = A.Fake<IGenericRepository<TeamLeaderboardEntity>>();
            _teamMemberRepository = A.Fake<IGenericRepository<TeamMemberLeaderboardEntity>>();
            _starpointLeaderboardRepository = A.Fake<IGenericRepository<StarpointEntity>>();
            _logger = A.Fake<ILogger<StarpointMessageService>>();
        }

        [Fact]
        public async Task StarpointMessageService_AddStarpoints_ReturnsTrue()
        {
            // Arrange
            var sut = new StarpointMessageService(_teamRepository, _teamMemberRepository, _starpointLeaderboardRepository, _logger);
            var fakeResponse = A.Fake<Response>();
            var fakeModel = new StarpointLeaderboardModel
            {
                TeamMemberId = Guid.NewGuid(),
                Starpoints = 0,
                CreatedDate = DateTime.UtcNow
            };

            A.CallTo(() =>
                    _teamMemberRepository.Get(A<Expression<Func<TeamMemberLeaderboardEntity, bool>>>.Ignored,
                        A<Func<IQueryable<TeamMemberLeaderboardEntity>, IOrderedQueryable<TeamMemberLeaderboardEntity>>>
                            .Ignored, A<string>.Ignored))
                .Returns(A.CollectionOfDummy<TeamMemberLeaderboardEntity>(5));

            A.CallTo(() => _teamRepository.Get(A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>
                        .Ignored, A<string>.Ignored))
                .Returns(new List<TeamLeaderboardEntity>
                {
                    new TeamLeaderboardEntity
                    {
                        EndDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            A.CallTo(() => _teamMemberRepository.Update(A<TeamMemberLeaderboardEntity>.Ignored))
                .Returns(fakeResponse);

            // Act
            var response = await sut.AddStarpoints(fakeModel);

            // Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async Task StarpointMessageService_RemoveStarpoints_ReturnsTrue()
        {
            // Arrange
            var sut = new StarpointMessageService(_teamRepository, _teamMemberRepository, _starpointLeaderboardRepository, _logger);
            var fakeResponse = A.Fake<Response>();
            var fakeModel = new StarpointLeaderboardModel
            {
                TeamMemberId = Guid.NewGuid(),
                Starpoints = 0,
                CreatedDate = DateTime.UtcNow
            };

            A.CallTo(() =>
                    _teamMemberRepository.Get(A<Expression<Func<TeamMemberLeaderboardEntity, bool>>>.Ignored,
                        A<Func<IQueryable<TeamMemberLeaderboardEntity>, IOrderedQueryable<TeamMemberLeaderboardEntity>>>
                            .Ignored, A<string>.Ignored))
                .Returns(A.CollectionOfDummy<TeamMemberLeaderboardEntity>(5));

            A.CallTo(() => _teamRepository.Get(A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>
                        .Ignored, A<string>.Ignored))
                .Returns(new List<TeamLeaderboardEntity>
                {
                    new TeamLeaderboardEntity
                    {
                        EndDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            A.CallTo(() => _teamMemberRepository.Update(A<TeamMemberLeaderboardEntity>.Ignored))
                .Returns(fakeResponse);

            // Act
            var response = await sut.AddStarpoints(fakeModel);

            // Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async Task StarpointMessageService_AddStarpointsToBackup_ReturnsTrue()
        {
            // Arrange
            var sut = new StarpointMessageService(_teamRepository, _teamMemberRepository, _starpointLeaderboardRepository, _logger);
            var fakeResponse = A.Fake<Response>();
            var fakeModel = new StarpointLeaderboardModel
            {
                TeamMemberId = Guid.NewGuid(),
                Starpoints = 0,
                CreatedDate = DateTime.UtcNow
            };
           
            A.CallTo(() =>
                    _starpointLeaderboardRepository.Get(A<Expression<Func<StarpointEntity, bool>>>.Ignored,
                     A<Func<IQueryable<StarpointEntity>, IOrderedQueryable<StarpointEntity>>>
                            .Ignored, A<string>.Ignored))
                .Returns(A.CollectionOfDummy<StarpointEntity>(5)
            );

            A.CallTo(() => _teamRepository.Get(A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>
                        .Ignored, A<string>.Ignored))
                .Returns(new List<TeamLeaderboardEntity>
                {
                    new TeamLeaderboardEntity
                    {
                        EndDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            A.CallTo(() => _starpointLeaderboardRepository.Update(A<StarpointEntity>.Ignored))
                .Returns(fakeResponse);

            // Act
            var response = await sut.AddStarpointsToBackup(fakeModel);

            // Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async Task StarpointMessageService_RemoveStarpointsFromBackup_ReturnsTrue()
        {
            // Arrange
            var sut = new StarpointMessageService(_teamRepository, _teamMemberRepository, _starpointLeaderboardRepository, _logger);
            var fakeResponse = A.Fake<Response>();
            var fakeModel = new StarpointLeaderboardModel
            {
                TeamMemberId = Guid.NewGuid(),
                Starpoints = 0,
                CreatedDate = DateTime.UtcNow
            };

           A.CallTo(() =>
                    _starpointLeaderboardRepository.Get(A<Expression<Func<StarpointEntity, bool>>>.Ignored,
                        A<Func<IQueryable<StarpointEntity>, IOrderedQueryable<StarpointEntity>>>
                            .Ignored, A<string>.Ignored))
                .Returns(A.CollectionOfDummy<StarpointEntity>(5));


            A.CallTo(() => _teamRepository.Get(A<Expression<Func<TeamLeaderboardEntity, bool>>>.Ignored,
                    A<Func<IQueryable<TeamLeaderboardEntity>, IOrderedQueryable<TeamLeaderboardEntity>>>
                        .Ignored, A<string>.Ignored))
                .Returns(new List<TeamLeaderboardEntity>
                {
                    new TeamLeaderboardEntity
                    {
                        EndDate = DateTime.UtcNow.AddDays(1)
                    }
                });

            A.CallTo(() => _starpointLeaderboardRepository.Update(A<StarpointEntity>.Ignored))
                .Returns(fakeResponse);

            // Act
            var response = await sut.RemoveStarpointsFromBackup(fakeModel);

            // Assert
            response.Should().BeFalse();
        }
    }
}