
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipAPI.Services.Membership
{
    using System.Linq;
    using MembershipApi.Domain.Interfaces;
    using MembershipApi.Domain.Repositories.Entities;
    using MembershipAPI.Domain.Interfaces;
    using MembershipAPI.Models;
    using MembershipAPI.Services.Membership.Models;
    using MembershipAPI.Services.UserInfo;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Logging;
    using Sigma.BoostApp.Contracts;

    public class MembershipService : IMembershipService
    {
        private readonly ITableOfType<TeamEntity> _teamRepository;

        private readonly ITableOfType<TeamMemberEntity> _teamMemberRepository;

        private readonly IUserInfoService _userInfoService;

        private readonly IDistributedCache _cache;

        private readonly ILogger<MembershipService> _logger;

        private IEnumerable<Team> _allTeams;

        private static IDictionary<int, List<UserInfo>> _teamMembers = new Dictionary<int, List<UserInfo>>();

        private readonly IStarpointsRepository _starpointsRepository;

        private const int DEFAULT_JPEG_QUALITY = 85;

        private static readonly PhotoSize DefaultMemberPhotoSize = PhotoSize.x96x64;

        public MembershipService(
            ITableOfType<TeamEntity> teamRepository, 
            ITableOfType<TeamMemberEntity> teamMemberRepository, 
            IUserInfoService userInfoService,
            IDistributedCache cache,
            ILogger<MembershipService> logger,
            IStarpointsRepository starpointsRepository)
        {
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _starpointsRepository = starpointsRepository;
            _userInfoService = userInfoService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TeamEntity> GetTeamByUserIdAsync(string userId)
        {
            var userMemberships = await _teamMemberRepository.ReadPartitionAsync("user-" + userId);

            _logger.LogDebug("User {UserId} memberships {@userMemberships} ", userId, userMemberships);

            var teamMembership = userMemberships.FirstOrDefault()?.RowKey ?? "0";

            var teamId = int.Parse(teamMembership);

            return await GetTeamByTeamIdAsync(teamId);
        }
        
        public async Task<TeamEntity> GetTeamByUserIdAsync(string userId, string competitionId)
        {
            var userMemberships = await _teamMemberRepository.ReadPartitionAsync("user-" + userId);

            _logger.LogDebug("User {UserId} memberships {@userMemberships} ", userId, userMemberships);

            var teamMembership = userMemberships.FirstOrDefault()?.RowKey ?? "0";

            var teamId = int.Parse(teamMembership);

            return await GetTeamByTeamIdAsync(teamId, competitionId);
        }

        public async Task<TeamEntity> GetTeamByTeamIdAsync(int teamId)
        {
            var teamIdLength = int.MaxValue.ToString().Length;
            var contestId = teamId.ToString();
            if(contestId.Length == teamIdLength)
            {
                contestId = contestId.Substring(0,5).PadRight(teamIdLength, '0');
            }
            var partitionKey = "competition-" + contestId;
            var entity = await _teamRepository.ReadAsync(partitionKey, teamId.ToString());
            return entity;
        }
        public async Task<TeamEntity> GetTeamByTeamIdAsync(int teamId, string competitionId)
        { 
            var contestId = competitionId;
            var partitionKey = "competition-" + contestId;
            var entity = await _teamRepository.ReadAsync(partitionKey, teamId.ToString());
            return entity;
        }

        public async Task<IEnumerable<UserInfo>> GetTeamMembersAsync(int teamId)
        {
            if (teamId == 0)
            {
                return Enumerable.Empty<UserInfo>();
            }

            if (_teamMembers.ContainsKey(teamId))
            {
                return _teamMembers[teamId];
            }

            var teamMembers = await _teamMemberRepository.ReadPartitionAsync("team-" + teamId);

            var teamMemberInfoTasks = teamMembers.Select(async x =>
            {

                var name = await _userInfoService.GetNameAsync(x.UserId, true);
                return new UserInfo
                {
                    UserId = x.UserId,
                    Id = x.UserId,
                    // Photo = x.Photo?.Base64,
                    DisplayName = name.ToString()
                };
            });

            var teamMemberInfos = await Task.WhenAll(teamMemberInfoTasks);

            // Cache it...
            _teamMembers[teamId] = teamMemberInfos.ToList();

            return teamMemberInfos;
        }

        public async Task<bool> RemoveAllMembersFromTeam(int teamId)
        {
            var teamMembers = await _teamMemberRepository.ReadTeamIDAsync(teamId.ToString());

            if (teamMembers.Any())
            {
                foreach (var member in teamMembers)
                {
                    await _teamMemberRepository.RemoveAsync(member.PartitionKey, member.RowKey);
                }
            }
          
            return true;
        }

        public async Task<bool> AssignMembership(int teamId, string userId, string role, string competitionID)
        {
            // Find all for this user and remove the ones not valid any more...
            var currentMemberships = await _teamMemberRepository.ReadPartitionAsync("user-" + userId);
            
            if (currentMemberships.Any(x => x.RowKey == teamId.ToString()))
            {
                _logger.LogInformation("There is already a membership for team {TeamId} and user {UserId}", teamId, userId);
                return true;
            }

            var userEntry = new TeamMemberEntity(userId, teamId)
            {
                Name = "User-Team",
                Role = role
            };

            var userResult = await _teamMemberRepository.SaveAsync(userEntry);
            _logger.LogInformation("SaveAsync: {@userResult}", userResult);

            var teamEntry = new TeamMemberEntity(teamId, userId)
            {
                Name = "Team-User",
                Role = role
            };

            var teamResult = await _teamMemberRepository.SaveAsync(teamEntry);
            _logger.LogInformation("SaveAsync: {@teamResult}", teamResult);

            var newMemberships = await _teamMemberRepository.ReadPartitionAsync("user-" + userId);
            // var result = await _teamMemberRepository.SetTeamMembershipAsync(teamId, userId);
            var success = newMemberships.Any(x => x.RowKey == teamId.ToString());

            var saveMember = await _starpointsRepository.AddOrEditUser(userId, competitionID, teamResult.TeamId);

            if(success)
            {
                _logger.LogInformation("REMOVE CACHE FOR TEAM MEMBERS");
                _teamMembers.Clear();
            }
            return success;
        }

        public async Task<bool> TeamMembershipExists(int teamId, string userId)
        {
            var userTeams = await _teamMemberRepository.ReadPartitionAsync("user-" + userId);
            var teamUsers = await _teamMemberRepository.ReadPartitionAsync("team-" + teamId);
            bool membershipExists = userTeams.Concat(teamUsers)
                .Where(x => x.RowKey == teamId.ToString() || x.RowKey == userId.ToString()).Any();

            return membershipExists;
        }

        public async Task<bool> RemoveMembership(int teamId, string userId)
        {
            var userTeams = await _teamMemberRepository.ReadPartitionAsync("user-" + userId);
            var teamUsers = await _teamMemberRepository.ReadPartitionAsync("team-" + teamId);
            var teamMemberships = userTeams.Concat(teamUsers)
                .Where(x => x.RowKey == teamId.ToString() || x.RowKey == userId.ToString());

            if (!teamMemberships.Any())
            {
                _logger.LogInformation("User with id {UserId} has no membership with team with id {teamId}.", userId, teamId);
                return false;
            }

            foreach (var teamMembership in teamMemberships)
            {
                await _teamMemberRepository.RemoveAsync(teamMembership.PartitionKey, teamMembership.RowKey);
            }

            _teamMembers.Clear();
            return true;
        }

        public async Task<FullTeamInfo> GetFullTeamInfo(int teamID)
        {
            var teamEntity = await GetTeamByTeamIdAsync(teamID);
            if(teamEntity == null)
            {
                return null;
            }

            var team = new FullTeamInfo(teamEntity);

            var membersEntity = await GetTeamMembersAsync(teamID);

            if(!membersEntity.Any())
            {
                return team;
            }

            var membersWithPhoto = await _userInfoService.GetMembersAsync(
                    userIds: membersEntity.Select(x => x.UserId),
                    silentlyFailNames: true,
                    size: DefaultMemberPhotoSize,
                    photoResizeCondition: PhotoResizeCondition.ResizeIfDefault,
                    photoResizeQuality: DEFAULT_JPEG_QUALITY
                );

            
            foreach (var member in membersWithPhoto)
            {
                var membersstarpoints = await _starpointsRepository.GetTotalStarpointsForUserByUserID(member.Id);
                team.TeamMembers.Add(new FullMemberInfo(member, membersstarpoints));
            }
            team.TotalStarpoints = team.TeamMembers.Select(m => m.Starpoints).Sum();
            return team;
        }
        public async Task<FullTeamInfo> GetFullTeamInfo(int teamID, int competitionId)
        {
            var teamEntity = await GetTeamByTeamIdAsync(teamID, competitionId.ToString());
            if (teamEntity == null)
            {
                return null;
            }

            var team = new FullTeamInfo(teamEntity);

            var membersEntity = await GetTeamMembersAsync(teamID);

            if (!membersEntity.Any())
            {
                return team;
            }

            var membersWithPhoto = await _userInfoService.GetMembersAsync(
                    userIds: membersEntity.Select(x => x.UserId),
                    silentlyFailNames: true,
                    size: DefaultMemberPhotoSize,
                    photoResizeCondition: PhotoResizeCondition.ResizeIfDefault,
                    photoResizeQuality: DEFAULT_JPEG_QUALITY
                );


            foreach (var member in membersWithPhoto)
            {
                var membersstarpoints = await _starpointsRepository.GetTotalStarpointsForUserByUserID(member.Id);
                team.TeamMembers.Add(new FullMemberInfo(member, membersstarpoints));
            }
            team.TotalStarpoints = team.TeamMembers.Select(m => m.Starpoints).Sum();
            return team;
        }

        
    }
}
