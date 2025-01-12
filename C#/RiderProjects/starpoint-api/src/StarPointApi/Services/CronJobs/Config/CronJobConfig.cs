using System;

namespace StarPointApi.Services.CronJobs.Config
{
    public class CronJobConfig<T> : ICronJobConfig<T>
    {
        public string CronExpression { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
    }
}