using MembershipApi.Domain.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Models
{
    public class FullTeamInfo
    {
        public string TeamID { get; set; }
        public string Name { get; set; }
        public string CompetitionID { get; set; }
        public string Description { get; set; }
        public string TeamImage { get; set; }
        public int TeamStarpointGoal { get; set; }
        public int TotalStarpoints { get; set; }
        public List<FullMemberInfo> TeamMembers { get; set; }
        public int Ranking { get; set; }
        public FullTeamInfo(TeamEntity teamEntity)
        {
            TeamID = teamEntity.TeamId;
            Name = teamEntity.TeamName;
            CompetitionID = teamEntity.CompetitionId;
            Description = teamEntity.GoalDescription;
            TeamImage = teamEntity.TeamImageUrl;
            TeamStarpointGoal = teamEntity.GoalPoints;
            TotalStarpoints = 0;
            TeamMembers = new List<FullMemberInfo>();
        }
    }
}
