using Newtonsoft.Json;
using System;

namespace MembershipAPI.Services.UserInfo.Models
{
    public readonly struct IdName : IEquatable<IdName>
    {
        public IdName(string id, Name name)
        {
            Id = id ?? throw new ArgumentNullException(paramName: nameof(id));
            Name = name;
        }

        public IdName(string id, string givenName, string surName, string displayName = null)
            : this(id, new Name(givenName: givenName, surName: surName, displayName: displayName))
        { }

        // Unique User ID
        public string Id { get; }

        // Users names
        [JsonIgnore]
        private Name Name { get; }

        // Usually a combo of givenname, middle initial, and surname
        public string DisplayName => Name.Display;

        // aka FirstName
        public string GivenName => Name.Given;

        // aka LastName
        public string SurName => Name.Sur;

        public override string ToString() => Id;

        public override bool Equals(object obj) => obj is IdName other && Equals(other);
        public bool Equals(IdName other) => this.Id == other.Id;
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;

        public static bool operator ==(IdName left, IdName right) => left.Equals(right);
        public static bool operator !=(IdName left, IdName right) => !left.Equals(right);
    }
}
