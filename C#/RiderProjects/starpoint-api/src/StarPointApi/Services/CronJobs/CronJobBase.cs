using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Timer = System.Timers.Timer;

namespace StarPointApi.Services.CronJobs
{
    public abstract class CronJobBase : IHostedService, IDisposable
    {
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZoneInfo;
        private Timer _timer;

        protected CronJobBase(string cronExpression, TimeZoneInfo timeZoneInfo)
        {
            _expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            _timeZoneInfo = timeZoneInfo;
        }

        public virtual void Dispose()
        {
            _timer?.Dispose();
        }

        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleJob(cancellationToken);
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            await Task.CompletedTask;
        }

        protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                _timer = new Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose(); // reset and dispose timer
                    _timer = null;

                    if (cancellationToken.IsCancellationRequested) return;
                    await DoWork(cancellationToken);
                    await ScheduleJob(cancellationToken); // reschedule next
                };
                _timer.Start();
            }

            await Task.CompletedTask;
        }

        public abstract Task DoWork(CancellationToken cancellationToken);
    }
}