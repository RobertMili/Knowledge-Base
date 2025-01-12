using Sigma.BoostApp.Shared;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MembershipAPI.Policies
{
    public class TeamMemberHandler : AuthorizationHandler<TeamMemberRequirement, IEnumerable<string>>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
                  TeamMemberRequirement requirement,
                    IEnumerable<string> members)
        {
            if (UserIsPrivileged() || UserIsTeamMember())
                context.Succeed(requirement);

            return Task.CompletedTask;

            bool UserIsPrivileged() => context.User.IsInRole("Membership.ReadAllTeams");
            bool UserIsTeamMember() => members.Contains(context.User.GetUniqueUserId(), StringComparer.Ordinal);
        }
    }
}
