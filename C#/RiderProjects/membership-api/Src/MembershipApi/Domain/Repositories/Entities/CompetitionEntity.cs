using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MembershipApi.Domain.Repositories.Entities
{
    public class CompetitionEntity : TableEntity
    {
        public string CompetitionId { get; set; }

        public int CompetitionNumber { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
        public string ImageUrl { get; set; }

        public CompetitionEntity()
        {
        }

        public CompetitionEntity(string competitionId) : base("competition", competitionId)
        {
        }
    }
}