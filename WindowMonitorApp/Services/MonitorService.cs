using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using WindowMoniterApp.Commons;
using WindowMonitorApp.Commons;
using WindowMonitorApp.Enititys;
using WindowMonitorApp.IServices;

namespace WindowMonitorApp.Services
{
    public class MonitorService : IMonitorService
    {

        public MonitorService(MyFreeSql freeSql, ILogger<MonitorService> logger, IConfiguration configuration)
        {
            this._IFreeSql = freeSql;
            this._logger = logger;
            this._configuration = configuration;
            try
            {
                cpuLimit = Double.Parse(configuration["CpuLimit"]);
                memoryLimit = Double.Parse(configuration["MemomryLimit"]);
                diskLimit = Double.Parse(configuration["DiskLimit"]);

            }
            catch (Exception ex)
            {
                this._logger.LogError("获取系统运行配置信息异常", ex);
            }
        }
        public SystemInfo systeminfo = new SystemInfo();
        private readonly MyFreeSql _IFreeSql;
        private readonly ILogger<MonitorService> _logger;
        private readonly IConfiguration _configuration;
        private double cpuLimit = 0;
        private double memoryLimit = 0;
        private double diskLimit = 0;
        public void  StartMonitor()
        {
            //启动CPU使用率刷新线程
            Task.Factory.StartNew(() =>
            {
                Monitor();
            });
        }

        public void Monitor()
        {
            while (true)
            {
                try
                {
                    bool isNeedInsert = false;
                    ComputerRunInfo computerRunInfo = new ComputerRunInfo();
                    computerRunInfo.DateTime = DateTime.Now;
                    systeminfo.GetMemory();
                    var cpuCurrent = Math.Round(systeminfo.totalCPU.NextValue(), 2);
                    //systeminfo.totalCPU.Dispose();
                    var memoryCurrent = 1 - ((float)systeminfo.MemoryAvailable) / (float)systeminfo.PhysicalMemory;
                    string mem = string.Format("{0:##}%", memoryCurrent * 100);
                    string cpu = string.Format("{0:##}%", cpuCurrent);
                    Console.Out.WriteLineAsync($"cpu:{cpuCurrent}");
                    List<DiskDetailInfo> diskDetailInfos = new List<DiskDetailInfo>();
                    computerRunInfo.Id = Guid.NewGuid().ToString();
                    computerRunInfo.CpuLoadInfo = cpu;
                    computerRunInfo.CpuLoad = cpuCurrent;
                    computerRunInfo.MemLoad = Math.Round(memoryCurrent, 2);
                    computerRunInfo.MemLoadInfo = mem;
                    diskDetailInfos = systeminfo.GetComputerDiskRunInfo();
                    computerRunInfo.DiskDetailInfos = JsonConvert.SerializeObject(diskDetailInfos);
                    if (computerRunInfo.CpuLoad > cpuLimit)
                    {
                        isNeedInsert = true;
                    }
                    else if (memoryCurrent * 100 > memoryLimit)
                    {
                        isNeedInsert = true;
                    }
                    else
                    {
                        var find = diskDetailInfos.FirstOrDefault(p => p.DiskLoad > diskLimit);
                        if (find != null)
                        {
                            isNeedInsert = true;
                        }
                    }
                    if (isNeedInsert)
                    {
                         _IFreeSql._freeSql.GetRepository<ComputerRunInfo>().Insert(computerRunInfo);
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(1000);
                    this._logger.LogError("监控系统运行情况异常", ex);
                }

            }
        }

        public async Task DeleteHistoryInfo()
        {
            try
            {
                var date = DateTime.Now.AddDays(-7);

                this._IFreeSql._freeSql.Delete<ComputerRunInfo>().Where(p => p.DateTime < date);
            }
            catch (Exception ex)
            {
                this._logger.LogError("定期删除数据异常", ex);

            }
        }
    }
}
