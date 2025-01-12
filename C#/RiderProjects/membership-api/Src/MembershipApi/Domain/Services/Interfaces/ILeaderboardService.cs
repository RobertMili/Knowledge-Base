using MembershipAPI.Domain.Repositories.Entities;
using MembershipAPI.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Domain.Services.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<TeamLeaderboardDTO>> GetTeamLeaderboardByCompetitionID(int competitionID);
        Task<List<MemberLeaderboardDTO>> GetMemberLeaderboardByCompetitionID(int competitionID);
        Task<List<UserEntity>> GetRankingForUsers(int competitionID);
    }
}
