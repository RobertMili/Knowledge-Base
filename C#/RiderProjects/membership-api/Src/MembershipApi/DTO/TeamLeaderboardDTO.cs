using MembershipApi.Domain.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.DTO
{
    public class TeamLeaderboardDTO
    {
        public string TeamID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int Starpoints { get; set; }
        public int Ranking { get; set; }

        public TeamLeaderboardDTO(TeamEntity team, int starpoints)
        {
            TeamID = team.TeamId;
            Name = team.TeamName;
            Image = team.TeamImageUrl;
            Starpoints = starpoints;
            Ranking = team.Ranking;
        }
    }

}
