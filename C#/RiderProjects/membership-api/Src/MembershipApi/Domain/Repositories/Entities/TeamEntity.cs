using System;
using Microsoft.WindowsAzure.Storage.Table;
using Sigma.BoostApp.Contracts;

namespace MembershipApi.Domain.Repositories.Entities
{
    public class TeamEntity : TableEntity
    {
        public string CompetitionId { get; set; }

        public string TeamId { get; set; }

        public int TeamNumber { get; set; }

        public string TeamName { get; set; }
        
        public string TeamImageUrl { get; set; }

        public string GoalDescription { get; set; }

        public int GoalPoints { get; set; }
        
        public string GoalImageUrl { get; set; }
        public int Ranking { get; set; }

        public TeamEntity()
        {
        }

        public TeamEntity(string competitionId, int teamId)
            : base("competition-" + competitionId, teamId.ToString())
        {
        }
    }
}