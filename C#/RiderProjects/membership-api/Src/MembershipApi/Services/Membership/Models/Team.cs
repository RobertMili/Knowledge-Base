using System;

namespace MembershipAPI.Services.Membership.Models
{
    // For DB:
    public readonly struct Team : IEquatable<Team>
    {
        public Team(int id, string name, string imageUrl)
        {
            Id = id;
            Name = name;
            ImageUrl = imageUrl;
        }

        public Team(string name, string imageUrl)
        {
            Id = null;
            Name = name;
            ImageUrl = imageUrl;
        }

        /// <summary>
        /// Unique team id.
        /// </summary>
        /// <remarks>
        /// Note that the id cannot be null in the database.
        /// Thus if this property comes back as null from a db query, it means that no matching row existed.
        /// </remarks>
        public int? Id { get; }

        /// <summary>
        /// Team display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Team logo url.
        /// </summary>
        public string ImageUrl { get; }

        /// <summary>
        /// True if a db query returned nothing - i.e. data matching the query does not exist in the database.
        /// </summary>
        public bool IsDBNull() => !Id.HasValue;

        public override string ToString() => Id.ToString();

        public override bool Equals(object obj) => obj is Team other && Equals(other);
        public bool Equals(Team other) => this.Id == other.Id;
        public override int GetHashCode() => Id ?? 0;

        // Usefull for quick instantiation in dummy code etc...
        public static implicit operator Team((int Id, string Name) tuple)
            => new Team(id: tuple.Id, name: tuple.Name, imageUrl: null);
        public static implicit operator Team((int Id, string Name, string ImageUrl) tuple)
            => new Team(id: tuple.Id, name: tuple.Name, imageUrl: tuple.ImageUrl);

        public static bool operator ==(Team left, Team right) => left.Equals(right);
        public static bool operator !=(Team left, Team right) => !left.Equals(right);
    }
}
