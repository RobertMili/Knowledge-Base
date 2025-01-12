using Newtonsoft.Json;
using System;

namespace MembershipAPI.Models
{
    using Name = MembershipAPI.Services.UserInfo.Models.Name;
    using Member_UI = MembershipAPI.Services.UserInfo.Models.Member;

    public readonly struct Member : IEquatable<Member>
    {
        public Member(string id, Name name, string photo = null)
        {
            Id = id ?? throw new ArgumentNullException(paramName: nameof(id));
            Name = name;
            Photo = photo;
        }

        public Member(string id, string givenName, string surName, string displayName = null, string photo = null)
            : this(id: id, new Name(givenName: givenName, surName: surName, displayName: displayName), photo: photo)
        { }

        public Member(Member_UI member)
            : this (id: member.Id, name: member.Name, photo: member.Photo?.ToBase64(true))
        { }

        // Unique User ID
        public string Id { get; }

        // Users Names
        [JsonIgnore]
        public Name Name { get; }

        // Usually a combo of givenname, middle initial, and surname
        public string DisplayName => Name.Display;

        // aka FirstName
        public string GivenName => Name.Given;

        // aka LastName
        public string SurName => Name.Sur;

        // Base64 encoded profile photo
        public string Photo { get; }

        public override string ToString() => Id;

        public override bool Equals(object obj) => obj is Member other && Equals(other);
        public bool Equals(Member other) => this.Id == other.Id;
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;

        public static bool operator ==(Member left, Member right) => left.Equals(right);
        public static bool operator !=(Member left, Member right) => !left.Equals(right);

        public static implicit operator Member(Member_UI member) => new Member(member);
    }
}
