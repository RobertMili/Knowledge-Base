using BoostApp.ClassLibrary;

namespace LeaderboardApi.Services.Interfaces;

public interface ITeamMemberMessageService : IMessageService
{
    Task<bool> AddTeamMember(LeaderboardMessageModel model);
    Task<bool> UpdateTeamMember(LeaderboardMessageModel model);
    Task<bool> DeleteTeamMember(Guid teamId, Guid teamMemberId); 
}