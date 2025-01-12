using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LeaderboardApi.DAL.Repositories;
using LeaderboardApi.DTOs;
using LeaderboardApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace LeaderboardApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly IGenericRepository<TeamLeaderboardEntity> _teamRepository;
        private readonly IGenericRepository<TeamMemberLeaderboardEntity> _teamMemberRepository;

        public LeaderboardController(
            IGenericRepository<TeamLeaderboardEntity> teamRepository,
            IGenericRepository<TeamMemberLeaderboardEntity> teamMemberRepository)
        {
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
        }

        [HttpGet("team/{competitionId}")]
        public async Task<ActionResult<IEnumerable<TeamLeaderboardDto>>> GetTeamLeaderboard(int competitionId)
        {
            var teams = await _teamRepository.Get(
                t => t.PartitionKey == competitionId.ToString(),
                q => q.OrderByDescending(t => t.Timestamp),
                "Name,TeamId,ImageUrl,RowKey,PartitionKey");

            var teamMemberTasks = teams.Select(async t =>
            {
                var teamMembers = await _teamMemberRepository.Get(
                    tm => tm.PartitionKey == t.RowKey,
                    includeProperties: "Starpoints");

                var totalPoints = teamMembers.Sum(tm => tm.Starpoints);
                return new TeamLeaderboardDto
                {
                    TeamId = Guid.Parse(t.RowKey),
                    CompetitionId = int.Parse(t.PartitionKey),
                    Name = t.Name,
                    ImageUrl = t.ImageUrl,
                    TotalStarpoints = totalPoints
                };
            });

            var teamLeaderboard = await Task.WhenAll(teamMemberTasks);

            return Ok(teamLeaderboard.OrderByDescending(t => t.TotalStarpoints));
        }


        [HttpGet("teammembers/{competitionId}")]
        public async Task<ActionResult<IEnumerable<GetTeamMemberLeaderboardDTO>>> GetTeamMemberLeaderboard(
            int competitionId)
        {
            var teams = await _teamRepository.Get(
                t => t.PartitionKey == competitionId.ToString(),
                q => q.OrderByDescending(t => t.Timestamp),
                "PartitionKey,RowKey");

            var teamMemberTasks = teams.Select(async t =>
            {
                var teamMembers = await _teamMemberRepository.Get(
                    tm => tm.PartitionKey == t.RowKey,
                    q => q.OrderByDescending(tm => tm.Starpoints));
                return teamMembers.Select(tm =>
                {
                    return new GetTeamMemberLeaderboardDTO
                    {
                        TeamMemberId = tm.RowKey,
                        TeamId = tm.PartitionKey,
                        Name = tm.Name,
                        Starpoints = tm.Starpoints
                    };
                });
            });

            if (!teamMemberTasks.Any())
            {
                return NotFound();
            }

            var teamMemberLeaderboard = await Task.WhenAll(teamMemberTasks);
            var leaderboardResult = new List<GetTeamMemberLeaderboardDTO>();
            foreach (var teamMemberArray in teamMemberLeaderboard)
            {
                foreach (var getTeamMemberLeaderboardDto in teamMemberArray)
                {
                    leaderboardResult.Add(getTeamMemberLeaderboardDto);
                }
            }

            return Ok(leaderboardResult);
        }
    }
}