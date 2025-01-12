using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Domain.Repositories.Entities
{
    public class UserEntity : TableEntity
    {
        public string ID => RowKey;
        public string UserID => PartitionKey;
        public string TeamID { get; set; }
        public string CompetitionID { get; set; }
        public int TotalStarpoints { get; set; }
        public int Ranking { get; set; }
    }
}
