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
using System.Threading.Tasks;
using Sigma.BoostApp.Contracts;
using MembershipAPI.Models;
using MembershipApi.Domain.Repositories.Entities;
using MembershipAPI.Extensions;
using MembershipAPI.Domain.Interfaces;
using MembershipAPI.Domain.Repositories.Entities;
using MembershipAPI.Domain.Services.Interfaces;
using MembershipAPI.DTO;
using MembershipApi.Services;

// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-2.2

namespace MembershipAPI.Controllers
{
    // using Me = Models.Me;
    // using Member = Models.Member;

    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = nameof(Sigma.BoostApp.Shared.Authorization.Policies.HasApprovedUserIdRequirement))]
    public class MembershipController : ControllerBase
    {
        private const int DEFAULT_JPEG_QUALITY = 85;
        private static readonly PhotoSize DefaultMemberPhotoSize = PhotoSize.x96x64; // == 96x96 with fallback to 64x64

        private readonly IAuthorizationService _authorization;
        private readonly IMembershipService _membershipService;
        private readonly IUserInfoService _userInfoService;
        private readonly IStarpointsRepository _starpointsRepository;
        private readonly ILeaderboardService _leaderboardService;
        private readonly CompetitionService contestService;


        public MembershipController(IAuthorizationService authorization, IMembershipService membershipService, IUserInfoService userInfoService, IStarpointsRepository starpointsRepository, ILeaderboardService leaderboardService, CompetitionService contestService)
        {
            _membershipService = membershipService;
            _userInfoService = userInfoService;
            _authorization = authorization;
            _starpointsRepository = starpointsRepository;
            _leaderboardService = leaderboardService;
            this.contestService = contestService;
        }

        /// <summary>
        /// Get basic info about yourself (the authenticated user).
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<Me>> GetMe()
        {
            var userId = User.GetUniqueUserId();
            var hasAdmin = User.IsInRole("boost-app-role-admin");
            var teamInfo = await _membershipService.GetTeamByUserIdAsync(userId);
            var name = await _userInfoService.GetNameAsync(userId, silentlyFail: true);
            var points = await _starpointsRepository.GetTotalStarpointsForUserByUserID(userId);

            return new Me
            {
                DisplayName =  name.Display,
                GivenName = name.Given,
                SurName = name.Sur,
                StarPoints = points,
                TeamId = Convert.ToInt32(teamInfo?.TeamId),
                UserId = userId,
                IsAdmin = hasAdmin
            };
        }

        // Missing  Challenges and Ranking
        [HttpGet("all/users")]
        public async Task<ActionResult<List<FullUserInfoDTO>>> GetAllUsers(int competitionID)
        {
            var users = await _starpointsRepository.GetUserEntityByCompetitionID(competitionID.ToString());
            var userInfo = new List<FullUserInfoDTO>();
            

            foreach (var user in users)
            {
                var teams = await _membershipService.GetTeamByTeamIdAsync(Convert.ToInt32(user.TeamID), competitionID.ToString());
                var userRank = await _leaderboardService.GetRankingForUsers(Convert.ToInt32(teams.CompetitionId));
                var displayName = await _userInfoService.GetNameAsync(user.UserID, true);
                userInfo.Add(new FullUserInfoDTO { 
                    TeamName = teams.TeamName,
                    UserID = user.ID, 
                    TeamID = teams.TeamId,
                    StarPoints = user.TotalStarpoints,
                    DisplayName = displayName.ToString(),
                    Ranking = userRank.FindIndex(x => x.ID == user.ID) + 1,
                    Challenges = 1
                });
            }

            return userInfo.OrderByDescending(x => x.StarPoints).ToList();
        }
        
        /// <summary>
        /// Get your profile photo.
        /// </summary>
        /// <param name="size">Desired photo size, or null to let server decide. (Recommended to stick with standard Ms Graph sizes)</param>
        /// <param name="quality">JPEG quality to use when resizing</param>
        [HttpGet("me/photo")]
        public async Task<ActionResult<object>> GetPhoto(
            [FromQuery]
                string size = null,
            [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
                int? quality = DEFAULT_JPEG_QUALITY
            )
        {
            PhotoSize.TryParse(size, out var photoSize);
            Photo? photoMaybe =
                await _userInfoService.GetPhotoAsync(
                    userId: User.GetUniqueUserId(),
                    size: photoSize,
                    resizeCondition: PhotoResizeCondition.ResizeIfDefault,
                    resizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

            return new { photo = photoMaybe?.ToBase64(true) };
        }

        /// <summary>
        /// Get the (first) team for which the authenticated user is a member.
        /// </summary>
        [HttpGet("me/team/{competitionId}")]
        public async Task<ActionResult<TeamInfo>> GetTeam(int competitionId)
        {
            var team = await _membershipService.GetTeamByUserIdAsync(User.GetUniqueUserId(), competitionId.ToString());

            return new ActionResult<TeamInfo>(team.GetMinimalInfo());
        }

        /*
         * TODO: add parameter string competitionid to GetTeamDetailed() and pass it as an argument to GetTeamByUserIdAsync(User.GetUniqueUserId(), competitionid)
         */
        /// <summary>
        /// Get the (first) team for which the authenticated user is a member, with detailed information.
        /// </summary>
        //[HttpGet("me/team/detailed")]
        //public async Task<ActionResult<TeamEntity>> GetTeamDetailed()
        //{
        //    var team = await _membershipService.GetTeamByUserIdAsync(User.GetUniqueUserId());

        //    return new ActionResult<TeamEntity>(team);
        //}


        /*
         * TODO: GetMembers() isn't working because _membershipService.GetTeamByUserIdAsync(userid) requires a competitionid to work, you could also get the team by accessing the teamid property on TeamMemberEntity
         */
        /// <summary>
        /// Get the list of team members for the authenticated user.
        /// </summary>
        //[HttpGet("me/members")]
        //public async Task<ActionResult<IEnumerable<IdName>>> GetMembers()
        //    => new ActionResult<IEnumerable<IdName>>(
        //        await _userInfoService.GetIdNamesAsync(
        //            (await GetTeamMembersForUser(User.GetUniqueUserId())).Select(x => x.UserId),
        //            silentlyFailNames: true)
        //        );

        /*
         * TODO: GetMemberPhoto([FromQuery] string size = null, [FromQuery, Range] int? quality = DEFAULT_JPEG_QUALITY)
         * isn't working because _membershipService.GetTeamByUserIdAsync(userid) requires a competitionid to work.
         * You could also get the team by accessing the teamid property on TeamMemberEntity 
         */
        /// <summary>
        /// Get a list of Base64 encoded profile photos of team members for the authenticated user.
        /// </summary>
        /// <param name="size">Desired photo size, or null to let server decide. (Recommended to stick with standard Ms Graph sizes)</param>
        /// <param name="quality">JPEG quality to use when resizing</param>
        /// <seealso cref="PhotoSize"/>
        //[HttpGet("me/members/photos")]
        //public async Task<IEnumerable<string>> GetMemberPhotos(
        //    [FromQuery]
        //        string size = null,
        //    [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
        //        int? quality = DEFAULT_JPEG_QUALITY)
        //{
        //    if (!PhotoSize.TryParse(size, out var photoSize))
        //        photoSize = DefaultMemberPhotoSize;

        //    var teamMembers = await GetTeamMembersForUser(User.GetUniqueUserId());
        //    Photo?[] photos =
        //        await _userInfoService.GetPhotosAsync(
        //            userIds: teamMembers.Select(x => x.UserId),
        //            size: photoSize,
        //            resizeCondition: PhotoResizeCondition.ResizeIfDefault,
        //            resizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

        //    return photos.Select(pn => pn is Photo p ? p.ToBase64(true) : null);
        //}

        /*
         * TODO: GetMembersFull([FromQuery] string size = null, [FromQuery, Range] int? quality = DEFAULT_JPEG_QUALITY)
         * isn't working because _membershipService.GetTeamByUserIdAsync(userid) required a competitionid to work.
         * You could also get the team by accessing the teamid property on TeamMemberEntity
         */
        /// <summary>
        /// Get a list of team members for the authenticated user, including Base64 encoded profile photos.
        /// </summary>
        /// <param name="size">Desired photo size, or null to let server decide. (Recommended to stick with standard Ms Graph sizes)</param>
        /// <param name="quality">JPEG quality to use when resizing</param>
        /// <seealso cref="PhotoSize"/>
        //[HttpGet("me/members/full")]
        //public async Task<IEnumerable<UserInfo>> GetMembersFull(
        //    [FromQuery]
        //        string size = null,
        //    [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
        //        int? quality = DEFAULT_JPEG_QUALITY)
        //{
        //    if (!PhotoSize.TryParse(size, out var photoSize))
        //        photoSize = DefaultMemberPhotoSize;

        //    var teamMembers = await GetTeamMembersForUser(User.GetUniqueUserId());
        //    var members = await _userInfoService.GetMembersAsync(
        //            userIds: teamMembers.Select(x => x.UserId),
        //            silentlyFailNames: true,
        //            size: photoSize,
        //            photoResizeCondition: PhotoResizeCondition.ResizeIfDefault,
        //            photoResizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

        //    return members.Select(x => new UserInfo {
        //        DisplayName = x.Name.ToString(),
        //        UserId = x.Id,
        //        Id = x.Id,
        //        Photo = x.Photo?.Base64,
        //        Image = new ProfileImage
        //        {
        //            Data = x.Photo?.Base64,
        //            DataType = x.Photo?.MimeType
        //        }
        //    });
        //}

        // --------------------------------------------------------
      

                
        [HttpGet("teams/{teamid}/members")]
        public async Task<ActionResult<IEnumerable<IdName>>> GetMembers([BindRequired, FromRoute]int teamid)
            => new ActionResult<IEnumerable<IdName>>(
                await _userInfoService.GetIdNamesAsync(
                    (await _membershipService.GetTeamMembersAsync(teamid)).Select(x => x.UserId),
                    silentlyFailNames: true)
                );

        /*
         * TODO: GetMemberPhotos([BindRequired, FromRoute] string id, [FromQuery] string size = null, [FromQuery, Range] int? quality = DEFAULT_JPEG_QUALITY)
         * isn't working because _membershipService.GetTeamByUserIdAsync(userid) required a competitionid to work.
         * You could also get the team by accessing the teamid property on TeamMemberEntity
         */
        /// <summary>
        /// Get a list of Base64 encoded profile photos of team members for the specified user.
        /// </summary>
        /// <param name="id">UserId</param>
        /// <param name="size">Desired photo size, or null to let server decide. (Recommended to stick with standard Ms Graph sizes)</param>
        /// <param name="quality">JPEG quality to use when resizing</param>
        /// <seealso cref="PhotoSize"/>
        //[HttpGet("users/{id}/members/photos")]
        //public async Task<IEnumerable<string>> GetMemberPhotos(
        //    [BindRequired, FromRoute]
        //        string id,
        //    [FromQuery]
        //        string size = null,
        //    [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
        //        int? quality = DEFAULT_JPEG_QUALITY)
        //{
        //    if (!PhotoSize.TryParse(size, out var photoSize))
        //        photoSize = DefaultMemberPhotoSize;

        //    var teamMembers = await GetTeamMembersForUser(id);
        //    Photo?[] photos =
        //        await _userInfoService.GetPhotosAsync(
        //            userIds: teamMembers.Select(x => x.UserId),
        //            size: photoSize,
        //            resizeCondition: PhotoResizeCondition.ResizeIfDefault,
        //            resizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

        //    return photos.Select(pn => pn is Photo p ? p.ToBase64(true) : null); // <-- without header currently (user ToBase64(true) for header)
        //}

        /// <summary>
        /// Get a list of Base64 encoded profile photos of team members for the specified team.
        /// </summary>
        /// <param name="id">TeamId</param>
        /// <param name="size">Desired photo size, or null to let server decide. (Recommended to stick with standard Ms Graph sizes)</param>
        /// <param name="quality">JPEG quality to use when resizing</param>
        /// <seealso cref="PhotoSize"/>
        [HttpGet("teams/{id}/members/photos")]
//#if !DEBUG
//        [Authorize(Roles = "Membership.ReadAllPhotos")]
//#endif
        public async Task<IEnumerable<string>> GetMemberPhotos(
            [BindRequired, FromRoute]
                int id,
            [FromQuery]
                string size = null,
            [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
                int? quality = DEFAULT_JPEG_QUALITY)
        {
            if (!PhotoSize.TryParse(size, out var photoSize))
                photoSize = DefaultMemberPhotoSize;

            var members = await GetTeamMembersForTeam(id);
            Photo?[] photos =
                await _userInfoService.GetPhotosAsync(
                    userIds: members.Select(x => x.UserId),
                    size: photoSize,
                    resizeCondition: PhotoResizeCondition.ResizeIfDefault,
                    resizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

            return photos.Select(pn => pn is Photo p ? p.ToBase64(true) : null); // <-- without header currently (user ToBase64(true) for header)
        }

        private async Task<IEnumerable<UserInfo>> GetTeamMembersForUser(string userId)
        {
            var team = await _membershipService.GetTeamByUserIdAsync(userId);
            return await GetTeamMembersForTeam(int.Parse(team?.TeamId ?? "0"));
        }

        private async Task<IEnumerable<UserInfo>> GetTeamMembersForTeam(int teamId)
        {
            var members = await _membershipService.GetTeamMembersAsync(teamId);
            return members;
        }

        /// <summary>
        /// Get a list of team members for the specified team, including Base64 encoded profile photos.
        /// </summary>
        /// <param name="id">TeamId</param>
        /// <param name="size">Desired photo size, or null to let server decide. (Recommended to stick with standard Ms Graph sizes)</param>
        /// <param name="quality">JPEG quality to use when resizing</param>
        /// <seealso cref="PhotoSize"/>
        [HttpGet("teams/{id}/members/full")]
        public async Task<IEnumerable<UserInfo>> GetMembersFull(
            [BindRequired, FromRoute]
                int id,
            [FromQuery]
                string size = null,
            [FromQuery, Range(30, 100, ErrorMessage = "quality has a valid range of 30 to 100", ConvertValueInInvariantCulture = true)]
                int? quality = DEFAULT_JPEG_QUALITY)
        {
            if (!PhotoSize.TryParse(size, out var photoSize))
                photoSize = DefaultMemberPhotoSize;

            var userIds = await _membershipService.GetTeamMembersAsync(id);
            var members = await _userInfoService.GetMembersAsync(
                    userIds: userIds.Select(x => x.UserId),
                    silentlyFailNames: true,
                    size: photoSize,
                    photoResizeCondition: PhotoResizeCondition.ResizeIfDefault,
                    photoResizeQuality: quality ?? DEFAULT_JPEG_QUALITY);

            return members.Select(x => new UserInfo {
                DisplayName = x.Name.ToString(),
                UserId = x.Id,
                Id = x.Id,
                Photo = x.Photo?.Base64,
                Image = new ProfileImage
                {
                    Data = x.Photo?.Base64,
                    DataType = x.Photo?.MimeType
                }
            });
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
                    //task = _userInfoService.GetNamesAsync(teamUserIds, silentlyFail: true); // <--- could be a Task.WhenAll
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
