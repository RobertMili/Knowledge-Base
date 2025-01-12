using System;

namespace BoostApp.Shared
{
    public static class DateHelper
    {
        // TODO: dokumentera hur denna beter sig...
        public static int NumberOfDays(DateTimeOffset from, DateTimeOffset? to = default)
        {
            TimeSpan span;
            if (to is DateTimeOffset toValue)
            {
                span = toValue - from;
                // if the callee didnt specify a time of day, we should treat the 'to'-date as inclusive,
                // i.e. if the input looked something like: from = "2019-01-10" , to = "2019-01-15"
                // then the callee probably expects an int[6] to be returned.
                if (toValue.TimeOfDay.Ticks == 0)
                    return span.Days + 1;
            }
            else // to-value was not specified:
            {
                span = DateTime.UtcNow - from.UtcDateTime;
            }
            return (int)Math.Ceiling(span.TotalDays);
        }
    }
}
