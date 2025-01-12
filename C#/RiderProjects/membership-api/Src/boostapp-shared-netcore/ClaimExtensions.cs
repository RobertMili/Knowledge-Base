using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace BoostApp.Shared
{
    public static class ClaimExtensions
    {
        /// <summary>
        /// Tries to return a human readable verion of a claim's Type value (best effort).
        /// </summary>
        public static string Type_HumanReadable(this Claim c)
            => ClaimHelper.GetReadableClaimType(c.Type);

        public static bool TryGetValue(this ClaimsPrincipal claims, string claimType, out string value)
            => ClaimHelper.TryGetValue(claims, claimType, out value);

        // -----

        /// <summary>
        /// Try get a unique user ID claim from a ClaimsPrincipal.
        /// </summary>
        /// <param name="claims">The ClaimsPrincipal</param>
        /// <param name="claim">A claim that uniquely identifies the user</param>
        public static bool TryGetUniqueUserId(this ClaimsPrincipal claims, out Claim claim)
            => ClaimHelper.UserId.TryGet(claims: claims, claim: out claim);

        /// <summary>
        /// Try get a unique user ID claim from a JWT claim collection.
        /// </summary>
        /// <param name="claims">Collection of JWT claims</param>
        /// <param name="claim">A claim that uniquely identifies the user</param>
        public static bool TryGetUniqueUserId(this IEnumerable<Claim> claims, out Claim claim)
            => ClaimHelper.UserId.TryGet(claims: claims, claim: out claim);

        /// <summary>
        /// Get a unique user ID claim from a ClaimsPrincipal, or throw trying.
        /// </summary>
        /// <param name="claims">The ClaimsPrincipal</param>
        /// <returns>The objectId claim value (AAD unique object id)</returns>
        public static string GetUniqueUserId(this ClaimsPrincipal claims)
            => ClaimHelper.UserId.GetValue(claims: claims);

        /// <summary>
        /// Get a unique user ID string from a JWT claim collection, or throw trying.
        /// </summary>
        /// <param name="claims">Collection of JWT claims</param>
        /// <returns>The objectId claim value (AAD unique object id)</returns>
        public static string GetUniqueUserId(this IEnumerable<Claim> claims)
            => ClaimHelper.UserId.GetValue(claims: claims);

        // -----

        /// <summary>
        /// Try get objectId claim from a ClaimsPrincipal.
        /// </summary>
        /// <param name="claims">The ClaimsPrincipal</param>
        /// <param name="oidClaim">The objectId claim (AAD unique object id)</param>
        [Obsolete("Use TryGetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static bool TryGetAzureObjectId(this ClaimsPrincipal claims, out Claim oidClaim)
            => ClaimHelper.ObjectId.TryGet(claims: claims, claim: out oidClaim);

        /// <summary>
        /// Try get objectId claim from a JWT claim collection.
        /// </summary>
        /// <param name="claims">Collection of JWT claims</param>
        /// <param name="oidClaim">The objectId claim (AAD unique object id)</param>
        [Obsolete("Use TryGetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static bool TryGetAzureObjectId(this IEnumerable<Claim> claims, out Claim oidClaim)
            => ClaimHelper.ObjectId.TryGet(claims: claims, claim: out oidClaim);

        /// <summary>
        /// Get objectId claim from a ClaimsPrincipal, or throw trying.
        /// </summary>
        /// <param name="claims">The ClaimsPrincipal</param>
        /// <returns>The objectId claim value (AAD unique object id)</returns>
        [Obsolete("Use GetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static string GetAzureObjectId(this ClaimsPrincipal claims)
            => ClaimHelper.ObjectId.GetValue(claims: claims);

        /// <summary>
        /// Get objectId string from a JWT claim collection, or throw trying.
        /// </summary>
        /// <param name="claims">Collection of JWT claims</param>
        /// <returns>The objectId claim value (AAD unique object id)</returns>
        [Obsolete("Use GetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static string GetAzureObjectId(this IEnumerable<Claim> claims)
            => ClaimHelper.ObjectId.GetValue(claims: claims);

        // -----

        /// <summary>
        /// Try get subject claim from a ClaimsPrincipal.
        /// </summary>
        /// <param name="claims">The ClaimsPrincipal</param>
        /// <param name="subjectClaim">The sub claim (issuer unique NameIdentity)</param>
        [Obsolete("Use TryGetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static bool TryGetSubject(this ClaimsPrincipal claims, out Claim subClaim)
            => ClaimHelper.Subject.TryGet(claims: claims, claim: out subClaim);

        /// <summary>
        /// Try get subject claim from a JWT claim collection.
        /// </summary>
        /// <param name="claims">Collection of JWT claims</param>
        /// <param name="subjectClaim">The sub claim (issuer unique NameIdentity)</param>
        [Obsolete("Use TryGetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static bool TryGetSubject(this IEnumerable<Claim> claims, out Claim subClaim)
            => ClaimHelper.Subject.TryGet(claims: claims, claim: out subClaim);

        /// <summary>
        /// Get subject claim from a ClaimsPrincipal, or throw trying.
        /// </summary>
        /// <param name="claims">The ClaimsPrincipal</param>
        /// <returns>The sub claim value (issuer unique NameIdentity)</returns>
        [Obsolete("Use GetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static string GetSubject(this ClaimsPrincipal claims)
            => ClaimHelper.Subject.GetValue(claims: claims);

        /// <summary>
        /// Get subject string from a JWT claim collection, or throw trying.
        /// </summary>
        /// <param name="claims">Collection of JWT claims</param>
        /// <returns>The sub claim value (issuer unique NameIdentity)</returns>
        [Obsolete("Use GetUniqueUserId method instead, or suppress this if you are REALLY sure you know what you are doing.")]
        public static string GetSubject(this IEnumerable<Claim> claims)
            => ClaimHelper.Subject.GetValue(claims: claims);
    }
}
