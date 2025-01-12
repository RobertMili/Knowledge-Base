using Newtonsoft.Json;
using System;

namespace MembershipAPI.Services.UserInfo.Models
{
    public readonly struct Member : IEquatable<Member>
    {
        public Member(string id, Name name, Photo? photo = default)
        {
            Id = id ?? throw new ArgumentNullException(paramName: nameof(id));
            Name = name;
            Photo = photo;
        }

        public Member(string id, string givenName, string surName, string displayName = null, Photo? photo = default)
            : this(id: id, new Name(givenName: givenName, surName: surName, displayName: displayName), photo: photo)
        { }

        // Unique User ID
        public string Id { get; }

        // Users Names
        [JsonIgnore]
        public Name Name { get; }

        // Base64 encoded profile photo
        public Photo? Photo { get; }

        public override string ToString() => Id;

        public override bool Equals(object obj) => obj is Member other && Equals(other);
        public bool Equals(Member other) => this.Id == other.Id;
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;

        public static bool operator ==(Member left, Member right) => left.Equals(right);
        public static bool operator !=(Member left, Member right) => !left.Equals(right);
    }
}
