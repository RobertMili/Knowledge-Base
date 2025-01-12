using System;

namespace StarPointApi.Services.CronJobs.Config
{
    public interface ICronJobConfig<T>
    {
        string CronExpression { get; set; }
        TimeZoneInfo TimeZoneInfo { get; set; }
    }
}