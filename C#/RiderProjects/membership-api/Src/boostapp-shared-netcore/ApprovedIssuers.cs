using System;
using System.Linq;
using System.Security.Claims;

namespace BoostApp.Shared
{
    // ...this way of doing things.... can probably be improved a lot...
    public sealed class ApprovedIssuers
    {
        public ApprovedIssuers(params string[] approvedIssuers) // Defensive copy:
            => this.approvedIssuers = approvedIssuers.ShallowCopy() ?? Array.Empty<string>();

        private readonly string[] approvedIssuers;

        public bool IsApproved(Claim claim)
            => IsApproved(claim.Issuer.Trim('/'));

        public bool IsApproved(string issuer)
            => approvedIssuers.Contains(issuer, StringComparer.Ordinal);
    }
}
