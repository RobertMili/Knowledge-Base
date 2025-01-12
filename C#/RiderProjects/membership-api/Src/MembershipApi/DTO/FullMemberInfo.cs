
using Sigma.BoostApp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Models
{
    public class FullMemberInfo
    {
        public string UserID { get; set; }
        public string DisplayName { get; set; }
        public int Starpoints { get; set; }
        public string Photo { get; set; }

        public FullMemberInfo(Member member, int starpoints)
        {
            UserID = member.Id;
            DisplayName = member.DisplayName;
            Starpoints = starpoints;
            Photo = member.Photo;
        }
    }
}
