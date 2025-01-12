using System;
using Microsoft.Extensions.DependencyInjection;
using StarPointApi.Services.CronJobs.Config;

namespace StarPointApi.Services.CronJobs.ServiceExtention
{
    public static class CronJobServiceExtension
    {
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services,
            Action<ICronJobConfig<T>> options) where T : CronJobBase
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");

            var config = new CronJobConfig<T>();
            options.Invoke(config);

            if (string.IsNullOrWhiteSpace(config.CronExpression))
                throw new ArgumentNullException(nameof(CronJobConfig<T>.CronExpression),
                    @"Empty Cron Expression is not allowed.");

            services.AddSingleton<ICronJobConfig<T>>(config);
            services.AddHostedService<T>();
            return services;
        }
    }
}