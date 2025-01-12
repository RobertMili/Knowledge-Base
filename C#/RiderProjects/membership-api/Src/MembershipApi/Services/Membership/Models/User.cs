using System;

namespace MembershipAPI.Services.Membership.Models
{
    // For DB:
    public readonly struct User : IEquatable<User>
    {
        public User(string oid)
        {
            Oid = oid;
        }

        public string Oid { get; }

        public override string ToString() => Oid;

        public override bool Equals(object obj) => obj is User other && Equals(other);
        public bool Equals(User other) => this.Oid == other.Oid;
        public override int GetHashCode() => Oid?.GetHashCode() ?? 0;

        public static explicit operator User(string oid) => new User(oid);

        public static bool operator ==(User left, User right) => left.Equals(right);
        public static bool operator !=(User left, User right) => !left.Equals(right);
    }
}
