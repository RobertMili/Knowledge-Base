using MembershipAPI.Domain.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.DTO
{
    public class MemberLeaderboardDTO
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }

        public string Photo { get; set; }
        public int Starpoints { get; set; }
        public int Ranking { get; set; }
    }
}
