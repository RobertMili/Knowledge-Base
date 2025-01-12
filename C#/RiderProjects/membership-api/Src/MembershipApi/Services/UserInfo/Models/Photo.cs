using Newtonsoft.Json;
using System;

namespace MembershipAPI.Services.UserInfo.Models
{
    public readonly struct Photo : IEquatable<Photo>
    {
        public Photo(byte[] bytes, int size, string mimeType)
            : this(bytes, size, size, mimeType)
        { }
        public Photo(byte[] bytes, PhotoSize size, string mimeType)
            : this(bytes, size.Width, size.Height, mimeType)
        { }

        public Photo(byte[] bytes, int width, int height, string mimeType)
        {
            Bytes = bytes;
            Width = width;
            Height = height;
            MimeType = mimeType;
        }

        [JsonIgnore]
        public byte[] Bytes { get; }

        public int Height { get; }
        public int Width { get; }
        public string MimeType { get; }
        public string Base64 => ToBase64(false);

        [JsonIgnore]
        public string SizeString => $"{Width}x{Height}";
        [JsonIgnore]
        public PhotoSize Size => new PhotoSize(width: Width, height: Height);

        public string ToBase64(bool includeHeader)
            => Bytes?.Length > 0
            ? includeHeader
                ? GetBase64Header() + Convert.ToBase64String(Bytes)
                : Convert.ToBase64String(Bytes)
            : null;

        public string GetBase64Header()
            => "data:" + (MimeType ?? "image/*") + ";base64,";

        public override string ToString() => ToBase64(false);

        public override int GetHashCode()
            => Bytes?.GetHashCode() ?? 0;

        public override bool Equals(object obj)
            => obj is Photo other && Equals(other);

        public bool Equals(Photo other)
        {
            var b1 = Bytes;
            var b2 = other.Bytes;

            if (b1 == b2)
                return true;

            if (b1 == null | b2 == null) // bitwise OR intended (fewer branches > less calc)
                return false;

            if (b1.Length != b2.Length)
                return false;

            for (int i = 0; i < b1.Length; ++i) // TODO: optimize with Span
                if (b1[i] != b2[i])
                    return false;

            return true;
        }

        public static bool operator ==(Photo left, Photo right) => left.Equals(right);

        public static bool operator !=(Photo left, Photo right) => !left.Equals(right);
    }
}
