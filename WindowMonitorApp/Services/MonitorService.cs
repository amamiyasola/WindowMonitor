using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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
        public void StartMonitor()
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


        /// <summary>
        /// 删除历史数据
        /// </summary>
        /// <returns></returns>
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

        public async Task<IWorkbook> ExportMonitorInfoAsync(List<ComputerRunInfo> computerRunInfos)
        {
            try
            {
                if (computerRunInfos == null || computerRunInfos.Count == 0) return null;

                //创建工作簿
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet($"资源监控数据-{DateTime.Now.ToString("yyyyMMddhhmmssffff")}");

                //添加表头
                IRow tableHeader = sheet.CreateRow(0);
                var colNames = new List<string>()
            {
                "CPU占比", "内存占比", "时间", "磁盘百分比写入信息"
            };
                for (int i = 0; i < colNames.Count; i++)
                {
                    tableHeader.CreateCell(i).SetCellValue(colNames[i]);
                    // 自适应宽高
                    sheet.AutoSizeColumn(i);
                }

                // 导出全部
                for (int i = 0; i < computerRunInfos.Count; i++)
                {
                    // 跳过表头
                    var row = sheet.CreateRow(i + 1);

                    row.CreateCell(0).SetCellValue(computerRunInfos[i].CpuLoadInfo);
                    row.CreateCell(1).SetCellValue(computerRunInfos[i].MemLoadInfo);
                    row.CreateCell(2).SetCellValue(computerRunInfos[i].DateTime.ToString("yyyy-MM-dd hh:mm:ss"));
                    row.CreateCell(3).SetCellValue(computerRunInfos[i].DiskDetailInfos);

                }


                return workbook;

            }
            catch (Exception ex)
            {
                this._logger.LogError("导出资源监控数据异常:", ex);
                return null;
            }
        }


    }
}

