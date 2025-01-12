using Microsoft.AspNetCore.Authorization;

namespace BoostApp.Shared.Policies
{
    public sealed class HasApprovedUserIdRequirement : IAuthorizationRequirement
    { }
}
