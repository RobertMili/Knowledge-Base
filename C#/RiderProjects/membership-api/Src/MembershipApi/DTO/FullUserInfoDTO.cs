using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.DTO
{
    public class FullUserInfoDTO
    {
        public string UserID { get; set; }
        public string DisplayName { get; set; }
        public string TeamID { get; set; }
        public string TeamName { get; set; }
        public int StarPoints { get; set; }
        public int Ranking { get; set; }
        public int Challenges { get; set; }
    }
}
