using System.Linq;

namespace StarPointApi.Shared
{
    public static class StringExtensions
    {
        public static bool IsEmptyOrWhiteSpace(this string str)
        {
            return str.All(c => char.IsWhiteSpace(c));
        }
    }
}