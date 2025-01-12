using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BoostApp.Shared
{
    public abstract class ClaimHelper
    {
        // Doc: https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens

        public static ClaimHelper ObjectId { get; } = new Sealed(@"http://schemas.microsoft.com/identity/claims/objectidentifier");
        public static ClaimHelper Subject { get; } = new Sealed(ClaimTypes.NameIdentifier);
        public static ClaimHelper UserId { get; } = ObjectId; // <--- HERE is where we decide what "UserId" is --Nick.D.

        // -----

        public readonly string ClaimType;

        public bool TryGet(ClaimsPrincipal claims, out Claim claim)
            => TryGet(claims: claims, claimType: ClaimType, claim: out claim);

        public bool TryGet(IEnumerable<Claim> claims, out Claim claim)
            => TryGet(claims: claims, claimType: ClaimType, claim: out claim);

        public string GetValue(ClaimsPrincipal claims)
            => GetValue(claims: claims, claimType: ClaimType);

        public string GetValue(IEnumerable<Claim> claims)
            => GetValue(claims: claims, claimType: ClaimType);

        // -----

        /// <summary>
        /// Tries to return a human readable verion of a claim's Type value (best effort).
        /// </summary>
        public static string GetReadableClaimType(string claimType)
            => claimType.LastIndexOf('/').Declare(out int i) > 0
            ? claimType.Substring(i + 1).Declare(out claimType) == "nameidentifier"
                ? "sub"
                : claimType
            : claimType;

        public static ClaimHelper GetHelperFor(string claimType)
            => string.IsNullOrWhiteSpace(claimType)
            ? throw new ArgumentException("ClaimType is null or whitespace", paramName: nameof(claimType))
            : claimType == ObjectId.ClaimType
                ? ObjectId
            : claimType == Subject.ClaimType
                ? Subject
                : new Sealed(claimType);
        // ^this static method is a public ctor substitute (i.e. a factory method)
        // it affords us the possibility of implementing instance re-use and other fancy stuff should we want to.

        // ctor: implementations restricted to inner classes; Use the GetHelperFor method for public construction.
        private ClaimHelper(string claimType) => ClaimType = claimType;

        // -----

        public static bool TryGet(ClaimsPrincipal claims, string claimType, out Claim claim)
            => !string.IsNullOrEmpty((
            claim = claims.FindFirst(claimType) // <-- null if not found
            )?.Value);

        public static bool TryGet(IEnumerable<Claim> claims, string claimType, out Claim claim)
            => !string.IsNullOrEmpty((
            claim = claims.FirstOrDefault(c => c.Type == claimType)
            )?.Value);

        public static bool TryGetValue(ClaimsPrincipal claims, string claimType, out string value)
            => !string.IsNullOrEmpty(
            value = claims.FindFirst(c => c.Type == claimType)?.Value);

        public static string GetValue(ClaimsPrincipal claims, string claimType)
            => TryGet(claims: claims, claimType: claimType, claim: out var claim)
            ? claim.Value
            : throw ClaimNotFoundException(GetReadableClaimType(claimType));

        public static string GetValue(IEnumerable<Claim> claims, string claimType)
            => TryGet(claims: claims, claimType: claimType, claim: out var claim)
            ? claim.Value
            : throw ClaimNotFoundException(GetReadableClaimType(claimType));

        // -----

        private sealed class Sealed : ClaimHelper // sealing allows vtable optimizations. --Nick.D.
        {
            public Sealed(string claimType) : base(claimType) { }
        }

        private static Exception ClaimNotFoundException(string claimType)
            => new InvalidOperationException($"{claimType} claim not found (JWT).");

    }
}
