using MembershipApi.Domain.Repositories.Entities;
using MembershipApi.Services;
using MembershipAPI.Domain.Interfaces;
using MembershipAPI.Domain.Repositories.Entities;
using MembershipAPI.DTO;
using MembershipAPI.Services.Membership;
using MembershipAPI.Services.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Domain.Services.Interfaces
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IMembershipService _membershipService;
        private readonly IStarpointsRepository _starpointsRepository;
        private readonly IUserInfoService _userInfoService;
        TeamService _teamService;
        private const int DEFAULT_JPEG_QUALITY = 85;
        private static readonly PhotoSize DefaultMemberPhotoSize = PhotoSize.x96x64;

        public LeaderboardService(IMembershipService membershipService, IStarpointsRepository starpointsRepository, IUserInfoService userInfoService, TeamService teamService)
        {
            _membershipService = membershipService;
            _starpointsRepository = starpointsRepository;
            _userInfoService = userInfoService;
            _teamService = teamService;
        }
        public async Task<List<MemberLeaderboardDTO>> GetMemberLeaderboardByCompetitionID(int competitionID)
        {
            var members = await _starpointsRepository.GetUserEntityByCompetitionID(competitionID.ToString());

            if (!members.Any())
            {
                return null;
            }

            var membersPhoto = await _userInfoService.GetMembersAsync(
                     userIds: members.Select(x => x.ID),
                     silentlyFailNames: true,
                     size: DefaultMemberPhotoSize,
                     photoResizeCondition: PhotoResizeCondition.ResizeIfDefault,
                     photoResizeQuality: DEFAULT_JPEG_QUALITY
                 );

            var memberLeaderboard = new List<MemberLeaderboardDTO>();
            

            foreach (var member in members)
            {
                var photo = membersPhoto.Where(p => p.Id == member.ID).Select(p => p).FirstOrDefault();
                if (photo.Name.Display != null)
                { 
                    memberLeaderboard.Add(new MemberLeaderboardDTO
                    {
                        ID = member.ID,
                        Photo = photo.Photo.ToString(),
                        Starpoints = member.TotalStarpoints,
                        DisplayName = photo.Name.Display
                    });
                }
            }

            var memberSorted = memberLeaderboard.OrderByDescending(t => t.Starpoints).ToList();

            for (int i = 0; i < memberSorted.Count; i++)
            {
                memberSorted[i].Ranking = i + 1;
            }

            return memberSorted;
        }

        public async Task<List<TeamLeaderboardDTO>> GetTeamLeaderboardByCompetitionID(int competitionID)
        {
            var teamEntities = await _teamService.GetTeamEntities(competitionID.ToString());

            if (!teamEntities.Any())
            {
                return null;
            }

            var teamLeaderboard = new List<TeamLeaderboardDTO>();
            foreach (var team in teamEntities)
            {
                var starpoints = await _starpointsRepository.GetTotalStarpointsForTeamByTeamID(team.TeamId);
                teamLeaderboard.Add(new TeamLeaderboardDTO(team, starpoints));

            }
            teamLeaderboard.OrderByDescending(t => t.Starpoints).ToList();

            for (int i = 0; i < teamLeaderboard.Count; i++)
            {
                teamLeaderboard[i].Ranking = i + 1;
            }

            return teamLeaderboard.OrderByDescending(t => t.Starpoints).ToList();
        }

        public async Task<List<UserEntity>> GetRankingForUsers(int competitionID)
        {
            var users = await _starpointsRepository.GetAllUserEntities();
            
            users.OrderByDescending(t => t.TotalStarpoints).ToList();
            
            for (int i = 0; i < users.Count; i++)
            {
                users[i].Ranking = i + 1;
            }

            return users.OrderByDescending(t => t.TotalStarpoints).ToList();
        }
    }
}
