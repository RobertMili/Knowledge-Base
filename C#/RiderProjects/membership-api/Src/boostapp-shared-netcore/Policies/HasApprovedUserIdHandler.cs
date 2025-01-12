using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace BoostApp.Shared.Policies
{
    // mostly an excercise in writing authorization handlers...
    public sealed class HasApprovedUserIdHandler : AuthorizationHandler<HasApprovedUserIdRequirement>
    {
        public HasApprovedUserIdHandler(ApprovedIssuers sigmaIssuers)
            => Issuers = sigmaIssuers;

        private ApprovedIssuers Issuers { get; }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasApprovedUserIdRequirement requirement)
        {
            if (context.User.Claims.TryGetUniqueUserId(out var claim) && Issuers.IsApproved(claim))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
