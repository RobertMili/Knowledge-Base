using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;

namespace StarPointApi.Shared
{
    public static class DateTimeExtensions
    {
        public static bool IsBefore(this DateTime date, DateTime otherDate)
            => date < otherDate;

        public static DateTime FirstDateOfMonth(this DateTime date)
            => new DateTime(date.Year, date.Month, 1);

        public static DateTime FirstDateOfYear(this DateTime date)
            => new DateTime(date.Year, 1, 1);

        public static DateTime FirstDateOfWeek(this DateTime date)
            => new DateTime(date.Year, date.Month, date.Day).AddDays(date.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : -(int) date.DayOfWeek + 1);

        public static IEnumerable<DateTime> AllMonthsBetween(this DateTime date, DateTime otherDate)
        {
            var dates = new List<DateTime>();
            var current = date.FirstDateOfMonth();
            while (current <= otherDate)
            {
                dates.Add(current);
                current = current.AddMonths(1);
            }

            return dates;
        }

        public static IEnumerable<DateTime> AllYearsBetween(this DateTime date, DateTime otherDate)
        {
            var dates = new List<DateTime>();
            var current = date.FirstDateOfYear();
            while (current <= otherDate)
            {
                dates.Add(current);
                current = current.AddYears(1);
            }

            return dates;
        }
        public static IEnumerable<DateTime> AllWeeksBetween(this DateTime date, DateTime otherDate)
        {
            var dates = new List<DateTime>();
            var current = date.FirstDateOfWeek();
            while (current <= otherDate)
            {
                dates.Add(current);
                current = current.AddDays(7);
            }
            
            return dates;
        }

        public static IEnumerable<DateTime> AllDaysBetween(this DateTime date, DateTime otherDate)
        {
            var dates = new List<DateTime>();
            var current = new DateTime(date.Year, date.Month,date.Day);
            while (current <= otherDate)
            {
                dates.Add(current);
                current = current.AddDays(1);
            }
            
            return dates;
        }
    }
}