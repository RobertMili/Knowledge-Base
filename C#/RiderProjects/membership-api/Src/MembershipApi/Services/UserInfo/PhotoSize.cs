using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MembershipAPI.Services.UserInfo
{
    // Equality ignores Fallback!
    public sealed class PhotoSize : IEquatable<PhotoSize>
    {
        // Profile photo sizes supported by MS Graph:
        // '48x48', '64x64', '96x96', '120x120', '240x240', '360x360','432x432', '504x504', '648x648'
        // Note that all of these sizes are not guaranteed to exist!
        // A profile might have some of the sizes without having all of them.
        public const string x48 = "48x48";
        public const string x64 = "64x64";
        public const string x96 = "96x96";
        public const string x120 = "120x120";
        public const string x240 = "240x240";
        public const string x360 = "360x360";
        public const string x432 = "432x432";
        public const string x504 = "504x504";
        public const string x648 = "648x648";

        // Fallback chains...
        public static readonly PhotoSize x48x64 = new PhotoSize(x48, x64);
        public static readonly PhotoSize x64x96 = new PhotoSize(x64, x96);
        public static readonly PhotoSize x96x64 = new PhotoSize(x96, x64);
        public static readonly PhotoSize x96x120 = new PhotoSize(x96, x120);
        public static readonly PhotoSize x120x96 = new PhotoSize(x120, x96);

        public static readonly PhotoSize x96x120x64 = new PhotoSize(x96, x120, x64);
        public static readonly PhotoSize x96x64x120 = new PhotoSize(x96, x64, x120);

        public static readonly PhotoSize x64x96x120x48 = new PhotoSize(x64, x96, x120, x48);
        public static readonly PhotoSize x96x120x64x48 = new PhotoSize(x96, x120, x64, x48);

        public int Height { get; }
        public int Width { get; }
        public string SizeString => $"{Width}x{Height}";
        public PhotoSize Fallback { get; }

        public int GetFallbackDepth()
        {
            const int LIVELOCKPROTECTION = 1000; // this insane number means we almost surely have a circular fallback
            int i = 0;
            for (var f = Fallback; f != null; f = f.Fallback)
                if (++i == LIVELOCKPROTECTION)
                    throw new System.Exception($"Fallback depth has exceeded acceptable limits ({LIVELOCKPROTECTION})! Is fallback circular?");
            return i;
        }

        public PhotoSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public PhotoSize(params (int Width, int Height)[] sizes)
        {
            if (sizes?.Length is int len && len > 0)
            {
                Width = sizes[0].Width;
                Height = sizes[0].Height;
                PhotoSize fallback = null;
                while (--len > 0)
                    fallback = new PhotoSize(sizes[len].Width, sizes[len].Height, fallback);
                Fallback = fallback;
            }
        }

        public PhotoSize(params string[] sizes)
        {
            if (sizes?.Length is int len && len > 0)
            {
                if (!TryParse(sizes[0], out int width, out int height))
                    throw new FormatException("Invalid size string");
                Width = width;
                Height = height;
                PhotoSize fallback = null;
                while (--len > 0)
                    fallback = new PhotoSize(sizes[len], fallback);
                Fallback = fallback;
            }
        }

        // This is private to minimize risk of constructing circular fallbacks
        private PhotoSize(string size, PhotoSize fallback)
        {
            if (!TryParse(size, out int width, out int height))
                throw new FormatException("Invalid size string");
            Width = width;
            Height = height;
            Fallback = fallback;
        }

        // This is private to minimize risk of constructing circular fallbacks
        private PhotoSize(int width, int height, PhotoSize fallback)
        {
            Width = width;
            Height = height;
            Fallback = fallback;
        }

        public override string ToString() => SizeString;

        public static implicit operator string(PhotoSize size) => size.SizeString;
        
        public static explicit operator PhotoSize(string size)
            => TryParse(size, out var photoSize)
            ? photoSize
            : throw new FormatException("Invalid size string");

        public override bool Equals(object obj)
            => Equals(obj as PhotoSize);

        public override int GetHashCode()
            => SizeString.GetHashCode();

        // Equality ignores Fallback!
        public bool Equals(PhotoSize other)
            => other != null && ((other.Width == Width) & (other.Height == Height));

        // --------------

        public static bool TryParse(string size, out PhotoSize photoSize)
        {
            if (size != null)
            {
                if (_msGraphSizes.TryGetValue(size, out photoSize))
                    return true;
                if (TryParse(size, out int width, out int height))
                {
                    photoSize = new PhotoSize(width: width, height: height);
                    return true;
                }
            }
            photoSize = default;
            return false;
        }

        public static bool TryParse(string size, out int width, out int height)
        {
            width = height = 0;
            return size?.IndexOf('x') is int x
                && int.TryParse(size.Substring(0, x), NumberStyles.None, NumberFormatInfo.InvariantInfo, out width) // digits only, no sign or anything
                && int.TryParse(size.Substring(x + 1), NumberStyles.None, NumberFormatInfo.InvariantInfo, out height);
        }

        public static bool IsMsGraphValid(string size)
            => _msGraphSizes.ContainsKey(size);

        private static readonly Dictionary<string, PhotoSize> _msGraphSizes = new []
            {
                x48,
                x64,
                x96,
                x120,
                x240,
                x360,
                x432,
                x504,
                x648,
            }
            .ToDictionary(x => x, x => new PhotoSize(x));
    }
}
