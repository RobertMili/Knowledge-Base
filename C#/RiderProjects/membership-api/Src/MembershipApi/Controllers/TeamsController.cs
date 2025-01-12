using MembershipAPI.Services.Membership;
using MembershipAPI.Services.UserInfo;
using MembershipAPI.Services.UserInfo.Models;
using Sigma.BoostApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Sigma.BoostApp.Shared.Authorization.Policies;
using Sigma.BoostApp.Contracts;
using System.Net.Http;
using Sigma.BoostApp.Contracts.GQL.Mutations;
using MembershipApi.Services;
using Microsoft.Extensions.Logging;
using MembershipAPI.Extensions;
using MembershipAPI.Models;

namespace MembershipAPI.Controllers
{
    using Member = Models.Member;

    [ApiController]
    [Route("competition-{competitionId}/teams")]
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

        [HttpGet("all")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IEnumerable<TeamInfo>> GetAllTeams([BindRequired, FromRoute]string competitionId)
        {
            var teams = await _teamService.GetAll(competitionId);
            return teams;
        }

        
        [HttpGet("{teamid}/goal")]
        public async Task<ActionResult<GoalInfo>> GetTeamGoal([BindRequired, FromRoute]string competitionId, [BindRequired, FromRoute]int teamid)
        {
            var goal = await _teamService.GetTeamGoal(competitionId, teamid);
            return goal;
        }
        //TODO Add parameter string competitionid to GetTeam() and then pass it as an argument to GetTeam([BindRequired, FromRoute] string userid)
        //[HttpGet("me")]
        //public async Task<ActionResult<TeamInfo>> GetTeam()
        //{
        //    return await GetTeam(userid: User.GetUniqueUserId());
        //}

        //TODO Add parameter string competitionid to GetTeam([BindRequired, FromRoute] string userid) and then pass it as an argument to GetTeamByUserIdAsync(userid, competitionid)
        //[HttpGet("users/{userid}")]
        //public async Task<ActionResult<TeamInfo>> GetTeam([BindRequired, FromRoute] string userid)
        //{
        //    var teamInfo = await _membershipService.GetTeamByUserIdAsync(userid);

        //    if (teamInfo == null)
        //        return NoContent();

        //    return Ok(teamInfo.GetMinimalInfo());
        //}


        [HttpGet("me/members/photos")]
        public async Task<IEnumerable<string>> GetMemberPhotos(
            [FromQuery] string size = null,
            [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)] int? quality = DEFAULT_JPEG_QUALITY)
        {
            if (!PhotoSize.TryParse(size, out var photoSize))
                photoSize = DefaultMemberPhotoSize;

            var teamInfo = await _membershipService.GetTeamByUserIdAsync(User.GetUniqueUserId());

            var teamMembers = await _membershipService.GetTeamMembersAsync(int.Parse(teamInfo?.TeamId ?? "0"));

            Photo?[] photos =
                await _userInfoService.GetPhotosAsync(
                    userIds: teamMembers.Select(x => x.UserId),
                    size: photoSize,
                    resizeCondition: PhotoResizeCondition.ResizeIfDefault,
                    resizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

            return photos.Select(pn => pn is Photo p ? p.ToBase64(true) : null);
        }
        
        //TODO add parameter string competitionid to GetTeam([BindRequired, FromRoute] int teamid) and then pass it as an argument to GetTeamByTeamIdAsync(teamid, competitionid)
        //[HttpGet("{teamid}")]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //public async Task<ActionResult<TeamInfo>> GetTeam([BindRequired, FromRoute]int teamid)
        //{
        //    var teamInfo = await _membershipService.GetTeamByTeamIdAsync(teamid);

        //    if (teamInfo == null)
        //        return NotFound();

        //    return Ok(teamInfo.GetMinimalInfo());
        //}

        [HttpGet("{teamid}/memberlist")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IEnumerable<UserInfo>> GetTeamMembers([BindRequired, FromRoute]int teamid)
        {
            var teamMembers = await _membershipService.GetTeamMembersAsync(teamid);

            var scheme = HttpContext.Request.Scheme;
            var host = HttpContext.Request.Host;

            return teamMembers.Select(x => new UserInfo{
                Id = x.Id,
                UserId = x.UserId,
                DisplayName = x.DisplayName,
                Image = new ProfileImage {
                    Data = $"{scheme}://{host}/teams/{teamid}/pics/{x.DisplayName}",
                    DataType = "text/url-list"
                }
            });
        }
        
        [AllowAnonymous]
        [HttpGet("{teamid}/pics/{displayName}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult> GetTeamMembers([BindRequired, FromRoute]int teamid, [BindRequired, FromRoute]string displayName)
        {
            var teamMembers = await _membershipService.GetTeamMembersAsync(teamid);

            var teamMember = teamMembers.FirstOrDefault(x => x.DisplayName == displayName);

            var pic = await _userInfoService.GetPhotoAsync(teamMember.UserId);

            if(pic == null)
            {
                pic = await GetDefaultTeamMemberPic(); 
            }
            
            var p = pic.GetValueOrDefault();

            return File(p.Bytes, p.MimeType);
        }

        private async Task<Photo> GetDefaultTeamMemberPic()
        {
            if(_defaultMemberPhoto.MimeType != null)
            {
                return _defaultMemberPhoto;
            }

            using(var client = new HttpClient())
            {
                var d = await client.GetByteArrayAsync("https://www.kindpng.com/picc/b/163/1636340.png");
                _defaultMemberPhoto = new Photo(d, 123, "image/png");
            }

            return _defaultMemberPhoto;
        }

        //TODO add paramater string competitionid to
        //TODO GetMembersFull([BindRequired, FromRoute] int teamid, [FromQuery] string size=null, [FromQuery, Range] int? quality = DEFAULT_JPEG_QUALITY)
        //TODO and pass it as an argument to GetTeamByTeamIdAsync(teamid, competitionid) and add a null check for var teamInfo
        //[HttpGet("{teamid}/members/full")]
        //public async Task<IEnumerable<Member>> GetMembersFull(
        //    [BindRequired, FromRoute]
        //        int teamid,
        //    [FromQuery]
        //        string size = null,
        //    [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
        //        int? quality = DEFAULT_JPEG_QUALITY)
        //{
        //    if (!PhotoSize.TryParse(size, out var photoSize))
        //        photoSize = DefaultMemberPhotoSize;

        //    var teamInfo = await _membershipService.GetTeamByTeamIdAsync(teamid);
        //    var teamMembers = await _membershipService.GetTeamMembersAsync(int.Parse(teamInfo.TeamId));
        //    var members = await _userInfoService.GetMembersAsync(
        //            userIds: teamMembers.Select(x => x.UserId),
        //            silentlyFailNames: true,
        //            size: photoSize,
        //            photoResizeCondition: PhotoResizeCondition.ResizeIfDefault,
        //            photoResizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

        //    return members.Select(m => new Member(m.Id, m.Name.ToString(), m.Photo.Value.Base64));
        //}

       

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<TeamInfo>> CreateTeam([BindRequired, FromBody] AssignTeamToContest payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(payload.TeamName))
            {
                return BadRequest();
            }

            var teams = await _teamService.GetAll(payload.ContestId);
            if (teams.Any(x => x.Name?.Equals(payload.TeamName, StringComparison.InvariantCultureIgnoreCase) == true))
            {
                return Conflict();
            }

            var result = await _teamService.AddTeam(payload);
            return Ok(result);
        }

        [HttpPost("{teamid}/memberlist")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<TeamInfo>> AssignTeamMembership(
            [BindRequired, FromRoute] int teamid,
            [BindRequired, FromBody] AssignUserToTeam payload)
        {
            var hasTeam = await _membershipService.GetTeamByUserIdAsync(payload.UserId, payload.ContestId);
            if (hasTeam != null)
                return BadRequest($"User with id {payload.UserId} is already member in a team with teamId {hasTeam.TeamId}");

            var teamInfo = await _membershipService.GetTeamByTeamIdAsync(teamid, payload.ContestId.ToString());
            if (teamInfo == null)
                return NotFound();

            var result = await _membershipService.AssignMembership(int.Parse(teamInfo.TeamId), payload.UserId, payload.Role, payload.ContestId);

            var newTeam = await _membershipService.GetTeamByUserIdAsync(payload.UserId, payload.ContestId.ToString());
            return Ok(newTeam.GetMinimalInfo());
        }

        [HttpPost("AssignTeamToContest")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<TeamInfo>> AssignTeamToContest([BindRequired, FromBody] AssignTeamToContest payload)
        {
            var allTeams = await GetAllTeams(payload.ContestId);
            var existingTeam = allTeams.FirstOrDefault(x => x.Name.Equals(payload.TeamName, StringComparison.InvariantCultureIgnoreCase));
            if (existingTeam != null)
            {
                return existingTeam;
            }

            return await CreateTeam(payload);
        }

        [HttpPut("{teamid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<TeamInfo>> UpdateTeam(
           [BindRequired, FromRoute] string competitionId,
           [BindRequired, FromRoute] int teamid,
           [BindRequired, FromBody] AssignTeamToContest payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(payload.TeamName))
            {
                return BadRequest();
            }

            var teams = await _teamService.GetAll(competitionId);
            if (teams.Where(x => x.Id != teamid.ToString()).Any(x => x.Name?.Equals(payload.TeamName, StringComparison.InvariantCultureIgnoreCase) == true))
            {
                return Conflict();
            }

            var result = await _teamService.UpdateTeam(teamid, payload);
            return Ok(result);
        }

        [HttpDelete("{teamId}/memberlist/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<TeamInfo>> RemoveTeamMembership(
           [BindRequired, FromRoute] int teamId,
           [BindRequired, FromRoute] string userId,
           [BindRequired, FromRoute] string competitionId
           )
        {
            //TODO: Rights and roles - for example, regular users may not delete other members.
            var team = await _membershipService.GetTeamByTeamIdAsync(teamId, competitionId);
            if (team == null)
                return NotFound($"Team with id {teamId} could not be found.");

            bool membershipExists = await _membershipService.TeamMembershipExists(teamId, userId);
            if (!membershipExists)
                return NotFound($"User with id {userId} has no membership in a team with id {teamId}.");

            var result = await _membershipService.RemoveMembership(teamId, userId);
            return Ok();
        }


        [HttpDelete("{teamid}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<TeamInfo>> DeleteTeam(
            [BindRequired, FromRoute] string competitionId,
            [BindRequired, FromRoute] int teamid)
        {
            var members = await _membershipService.GetTeamMembersAsync(teamid);
            if (members.Any())
            {
                try
                {
                    await _membershipService.RemoveAllMembersFromTeam(teamid);
                }
                catch
                {
                    return BadRequest();
                }

            }

            var result = await _teamService.DeleteTeam(competitionId, teamid);
            return Ok(result);
        }
        // TODO: the below method can be used to allow the above methods to work for users without roles (that are part of team) - requires some tweaking tho'
        // Takes care of the resource based authorization for the id-based endpoints
        private async Task<ActionResult<IEnumerable<T>>> AuthorizationHelper<T>(Task<IEnumerable<string>> userIdsTask, Func<IEnumerable<string>, Task<T[]>> getTFromUserIdFunc)
        {
            IEnumerable<string> teamUserIds = await userIdsTask;
            AuthorizationResult authorizationResult = await _authorization.AuthorizeAsync(User, teamUserIds, nameof(Policies.TeamMemberRequirement));

            if      (authorizationResult.Succeeded) return await SuccessResult();
            else if (User.Identity.IsAuthenticated) return new ForbidResult();
            else                                    return new ChallengeResult();

            async Task<T[]> SuccessResult()
            {
                Task<T[]> task = null;
                try
                {
                    task = getTFromUserIdFunc(teamUserIds);
                    return await task; // <-- awaiting unwraps the AggregateException, only returning the first exception thrown
                }
                // since the above might not be what we want, we do this to grab the full AggregateException
                catch when (task?.Exception != null)
                {
                    // TODO: log the entire AggregateException here.
                    var statusCodes = task.Exception.InnerExceptions.OfType<FailedToAquireUserInfoException>().Select(e => e.HttpStatusCode);
                    if (statusCodes.Contains(System.Net.HttpStatusCode.Unauthorized))
                    {
                        throw new Exception(message: "Internal Backend Error:" +
                            " UserInfoService has not been authorized to access Microsoft Graph users." +
                            " As a tenat admin, please go into AppRegistrations > [MembershipService] > API Permissions," +
                            " and ensure this app has been granted the 'Microsoft Graph > User.Read.All' permission.",
                            innerException: task.Exception);
                    }
                    else
                    {
                        throw task.Exception;
                    }
                }
            }
        }
    }
}
