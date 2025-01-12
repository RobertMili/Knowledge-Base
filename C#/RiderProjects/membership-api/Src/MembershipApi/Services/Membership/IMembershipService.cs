using System.Collections.Generic;
using System.Threading.Tasks;

namespace MembershipAPI.Services.Membership
{
    using MembershipApi.Domain.Repositories.Entities;
    using MembershipAPI.Models;
    using Sigma.BoostApp.Contracts;

    // Assumes the user only has one team (even though the database supports multi-team).
    public interface IMembershipService
    {
        Task<TeamEntity> GetTeamByUserIdAsync(string userId);
        Task<TeamEntity> GetTeamByUserIdAsync(string userId, string competitionId);

        Task<TeamEntity> GetTeamByTeamIdAsync(int teamId);
        Task<TeamEntity> GetTeamByTeamIdAsync(int teamId, string competitionId);

        Task<IEnumerable<UserInfo>> GetTeamMembersAsync(int teamId);

        Task<bool> AssignMembership(int id, string userId, string role, string competitionID);

        Task<bool> TeamMembershipExists(int id, string userId);

        Task<bool> RemoveMembership(int id, string userId);

        Task<bool> RemoveAllMembersFromTeam(int teamId);

        Task<FullTeamInfo> GetFullTeamInfo(int teamID);
        Task<FullTeamInfo> GetFullTeamInfo(int teamID, int competitionID);
    }
}
