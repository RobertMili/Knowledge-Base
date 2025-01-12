using Microsoft.AspNetCore.Authorization;

namespace MembershipAPI.Policies
{
    public sealed class TeamMemberRequirement : IAuthorizationRequirement
    { }
}
