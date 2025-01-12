using System;

namespace StarPointApi.Shared
{
    public static class TimeZoneHelper
    {
        public enum Country
        {
            Sweden
        }

        public static TimeZoneInfo GeTimeZoneInfoByCountry(Country country)
        {
            TimeZoneInfo timeZone = null;
            switch (country)
            {
                case Country.Sweden:
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(IsLinux()
                        ? "Europe/Stockholm"
                        : "W. Europe Standard Time");
                    break;
            }

            return timeZone;
        }

        private static bool IsLinux()
        {
            var p = (int) Environment.OSVersion.Platform;
            return p == 4 || p == 6 || p == 128;
        }
    }
}