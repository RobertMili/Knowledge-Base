using MembershipApi.Domain.Repositories.Entities;
using Sigma.BoostApp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Extensions
{
    public static class TeamHelper
    {
        public static TeamInfo GetMinimalInfo(this TeamEntity teamEntity)
        {
            if (teamEntity == null)
                return null;

            return new TeamInfo
            {
                Id = teamEntity.TeamNumber.ToString(),
                Name = teamEntity.TeamName ?? "Default",
                Image = new ProfileImage
                {
                    Data = teamEntity.TeamImageUrl,
                    DataType = "text/uri-list"
                }
            };
        }
    }
}
