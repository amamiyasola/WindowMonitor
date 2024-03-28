using Hangfire;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WindowMonitorApp.IServices;

namespace WindowMoniterApp
{
    public class WindowMonitorHostservice : IHostedService
    {
        private readonly IMonitorService _monitorService;
        public WindowMonitorHostservice(IMonitorService monitorService)
        {
            _monitorService = monitorService;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                Thread.Sleep(10000);
                RecurringJob.AddOrUpdate(() => this._monitorService.DeleteHistoryInfo(), Cron.Daily);
                //RecurringJob.AddOrUpdate();
            });

            //BackgroundJob.Schedule(() => this._monitorService.StartMonitor(), TimeSpan.FromSeconds(10));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

        }
    }
}
