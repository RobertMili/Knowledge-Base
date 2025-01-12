using MembershipAPI.Domain.Repositories.Entities;
using MembershipAPI.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Domain.Interfaces
{
    public interface IStarpointsRepository
    {
        Task<int> GetTotalStarpointsForTeamByTeamID(string teamID);
        Task<int> GetTotalStarpointsForUserByUserID(string userID);
        Task<UserEntity> AddOrEditUser(string userID, string competitionID, string teamID);
        Task<IList<UserEntity>> GetUserEntityByCompetitionID(string competitionID);
        Task<IList<UserEntity>> GetAllUserEntities();
    }
}
