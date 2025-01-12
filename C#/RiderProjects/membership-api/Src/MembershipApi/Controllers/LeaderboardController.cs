using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sigma.BoostApp.Contracts;
using System;
using System.Linq;
using MembershipApi.Services;
using Sigma.BoostApp.Shared.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using MembershipAPI.Domain.Services.Interfaces;
using MembershipAPI.DTO;

namespace MembershipAPI.Controllers
{
    [ApiController]
    [Route("leaderboard")]
    [Authorize(Policy = nameof(HasApprovedUserIdRequirement))]
    public class LeaderboardController : ControllerBase
    {
        private readonly CompetitionService _contestService;

        private readonly TeamService _teamService;

        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(CompetitionService contestService, TeamService teamService, ILeaderboardService leaderboardService)
        {
            _contestService = contestService;
            _teamService = teamService;
            _leaderboardService = leaderboardService;
        }

        [HttpGet("competitions/{competitionid}/teams")]
        public async Task<ActionResult<TeamLeaderboardDTO>> GetTeamLeaderboard(int competitionid)
        {
            var teamLeaderboard = await _leaderboardService.GetTeamLeaderboardByCompetitionID(competitionid);

            if(teamLeaderboard == null)
            {
                return BadRequest($"Can't find competition with CompetitionID: {competitionid}");
            }
            return Ok(teamLeaderboard);
        }

        [HttpGet("competitions/{competitionid}/members")]
        public async Task<ActionResult<MemberLeaderboardDTO>> GetMemberLeaderboard(int competitionid)
        {
            var memberLeaderboard = await _leaderboardService.GetMemberLeaderboardByCompetitionID(competitionid);
            if(memberLeaderboard == null)
            {
                return BadRequest($"Can't find competition with CompetitionID: {competitionid}");
            }
            return Ok(memberLeaderboard);
        }   

        private async Task<IList<ContestantInfo>> GetAllTeams(string contestId)
        {
            var teams = await _teamService.GetAll(contestId);

            return teams.Select((x, i) =>
             {
                 return new ContestantInfo
                 {
                     Id = x.Id,
                     Rank = i,
                     Score = i * 100,
                     ContestantType = "team",
                     DisplayName = x.Name,
                     Goal = new GoalInfo
                     {
                         Description = "The team goal description",
                         Image = new ProfileImage
                         {
                             Data = "https://www.dagensmedicin.se/globalassets/utbildning/kursbilder/bygga-team.-1000.jpg?w=800&scale=both",
                             DataType = "text/uri-list"
                         },
                          Score = i * 100 + 100
                     },
                     Image = x.Image
                 };
             }).ToList();
        }

        private IList<ContestantInfo> GetAllUsers(string competitionid)
        {
            return Enumerable.Range(1, 100).Select(x =>
             {
                 return new ContestantInfo
                 {
                     Id = "user-" + x,
                     Rank = x,
                     Score = x * 100,
                     ContestantType = "user",
                     DisplayName = "User " + x,
                     Goal = new GoalInfo
                     {
                         Description = "The user goal description",
                         Image = new ProfileImage
                         {
                         Data = "https://image.flaticon.com/icons/svg/272/272075.svg",
                         DataType = "text/uri-list"
                         },
                          Score = x * 100 + 10
                     },
                     Image = new ProfileImage
                     {
                         Data = "https://image.flaticon.com/icons/svg/272/272075.svg",
                         DataType = "text/uri-list"
                     }
                 };
             }).ToList();
        }
    }
}
