using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BoostApp.Shared
{
    // Various extensions that didn't deserve their own static class...
    public static class Extensions
    {
        // "hack" that lets me declare variables inside an expression
        // (this mehod doesn't actually do anything, it is just a workaround for a C# syntax limitation)
        public static T Declare<T>(this T value, out T variable)
            => (variable = value);

        /// <summary>
        /// Gets a value that indicates if the HTTP response was successful.
        /// </summary>
        /// <returns>
        /// True if statusCode was in the range 200-299; otherwise false.
        /// </returns>
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
            => (int)statusCode >= 200 && (int)statusCode <= 299;

        /// <summary>
        /// True if the string is non-null and has a length of zero, otherwise false.
        /// </summary>
        public static bool IsEmpty(this string s)
            => s != null && s.Length == 0;

        /// <summary>
        /// Creates a shallow copy of an array.
        /// </summary>
        /// <typeparam name="T">Array element type</typeparam>
        /// <returns>Null if the soruce was null; Otherwise a shallow copy of the source.</returns>
        public static T[] ShallowCopy<T>(this T[] source)
        {
            if (source == null)
                return null;

            var shallowCopy = new T[source.Length];
            source.CopyTo(shallowCopy, 0);
            return shallowCopy;
        }

        /// <summary>
        /// Same as GetSection, except it returns null instead of an empty section if the section does not exist.
        /// </summary>
        /// <param name="config">The IConfiguration</param>
        /// <param name="key">Key of the configuration section</param>
        /// <returns>The Microsoft.Extensions.Configuration.IConfigurationSection; or null if no such section exists</returns>
        public static IConfigurationSection GetSectionOrNull(this IConfiguration config, string key)
            => config.GetChildren().FirstOrDefault(section => string.Equals(section.Key, key, StringComparison.Ordinal));

        /// <summary>
        /// Retrieves a random index between 0 and Count (inclusive and exclusive).
        /// </summary>
        public static int RandomIndex<T>(this ICollection<T> source, Random random = null)
            => (random ?? ThreadStatic.Random).Next(source.Count);

        /// <summary>
        /// Retrieves a random item from a list.
        /// </summary>
        public static T RandomItem<T>(this IList<T> source, Random random = null)
            => source[source.RandomIndex(random)];
    }
}
