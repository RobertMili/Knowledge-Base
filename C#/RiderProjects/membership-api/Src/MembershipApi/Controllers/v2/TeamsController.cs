using MembershipApi.Services;
using MembershipAPI.Models;
using MembershipAPI.Services.Membership;
using MembershipAPI.Services.UserInfo;
using MembershipAPI.Services.UserInfo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sigma.BoostApp.Shared.Authorization.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MembershipAPI.Controllers.v2
{
    [ApiController]
    [Route("api/v2.0/[controller]")]
    [Authorize(Policy = nameof(HasApprovedUserIdRequirement))]
    public class TeamsController : ControllerBase
    {
        private const int DEFAULT_JPEG_QUALITY = 85;

        private static readonly PhotoSize DefaultMemberPhotoSize = PhotoSize.x96x64; // == 96x96 with fallback to 64x64

        private readonly IAuthorizationService _authorization;

        private readonly ILogger<TeamsController> _logger;

        private readonly IMembershipService _membershipService;

        private readonly IUserInfoService _userInfoService;

        private readonly TeamService _teamService;

        private static Photo _defaultMemberPhoto;

        public TeamsController(
            IAuthorizationService authorization,
            TeamService teamService,
            IMembershipService membershipService,
            IUserInfoService userInfoService,
            ILogger<TeamsController> logger)
        {
            _teamService = teamService;
            _membershipService = membershipService;
            _userInfoService = userInfoService;
            _authorization = authorization;
            _logger = logger;
        }

        [HttpGet("teamwithmembers/{teamId}/{competitionId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<FullTeamInfo>> GetTeamWithMembersByTeamID(int teamId, int competitionId)
        {
            var fullTeam = await _membershipService.GetFullTeamInfo(teamId, competitionId);
            if (fullTeam == null)
            {
                return NotFound($"Team with ID {teamId} dosn't exsist");
            }

            return Ok(fullTeam);
        }
    }
}
