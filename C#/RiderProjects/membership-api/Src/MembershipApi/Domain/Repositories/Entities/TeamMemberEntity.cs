using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MembershipApi.Domain.Repositories.Entities
{
    public class TeamMemberEntity : TableEntity
    {
        public string TeamId { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public TeamMemberEntity()
        {
        }

        public TeamMemberEntity(int teamId, string userId)
            : base("team-" + teamId.ToString(), userId)
        {
            UserId = userId;
            TeamId = teamId.ToString();
        }

        public TeamMemberEntity(string userId, int teamId)
            : base("user-" + userId, teamId.ToString())
        {
            UserId = userId;
            TeamId = teamId.ToString();
        }
    }
}