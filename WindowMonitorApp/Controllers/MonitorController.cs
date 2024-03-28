using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowMonitorApp.Commons;
using WindowMonitorApp.Enititys;
using WindowMonitorApp.IServices;
using WindowMonitorApp.Models;

namespace WindowMonitorApp.Controllers
{
    [Route("api/Monitor")]
    public class MonitorController : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly MyFreeSql _myFreeSql;
        private readonly IMonitorService _monitorService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MonitorController(ILogger<MonitorController> logger, MyFreeSql myFreeSql, IMonitorService monitorService, IWebHostEnvironment webHostEnvironment)
        {
            this._logger = logger;
            this._myFreeSql = myFreeSql;
            _monitorService = monitorService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("GetWindowMonitor")]
        public async Task<BaseResDto<List<ComputerRunInfo>>> GetWindowMonitorAsync(GetMonitorInfo getMonitorInfo)
        {
            var res = new BaseResDto<List<ComputerRunInfo>>()
            {
                Code = 0,
                Message = ""
            };
            try
            {
                var qeury = _myFreeSql._freeSql.Select<ComputerRunInfo>().WhereIf(getMonitorInfo.Cpu > 0, p => p.CpuLoad > getMonitorInfo.Cpu)
                    .WhereIf(getMonitorInfo.Memo > 0, p => p.MemLoad > getMonitorInfo.Memo).Where(p => p.DateTime > getMonitorInfo.DateTime).ToList();
                res.Data = qeury;

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                _logger.LogError("获取系统监控资源异常:", ex);
            }
            //var qeury = await _myFreeSql._freeSql.GetRepository<ComputerRunInfo>().WhereIf.ToListAsync();
            return res;
        }

        [HttpGet("DownloadWindowMonitor")]
        public async Task<FileStreamResult> DownloadWindowMonitorAsync(GetMonitorInfo getMonitorInfo)
        {
            try
            {
                var qeury = _myFreeSql._freeSql.Select<ComputerRunInfo>().WhereIf(getMonitorInfo.Cpu > 0, p => p.CpuLoad > getMonitorInfo.Cpu)
                    .WhereIf(getMonitorInfo.Memo > 0, p => p.MemLoad > getMonitorInfo.Memo).Where(p => p.DateTime > getMonitorInfo.DateTime).ToList();

                //List<ComputerRunInfo> Exportlist = ExcelHelper<List<ComputerRunInfo>>.OutPutExcel(qeury);//获取到list数据
                var workbook = await this._monitorService.ExportMonitorInfoAsync(qeury);//调用封装好的导出方法

                if (workbook != null)
                {
                    var path = Path.Combine(_webHostEnvironment.ContentRootPath, "export");
                    if (!Directory.Exists(path)) //没有此路径就新建
                    {
                        Directory.CreateDirectory(path);
                    }
                    var fileFullName = Path.Combine(path, $"{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.xlsx");

                    // 将表格写入文件流
                    FileStream creatStream = new FileStream(fileFullName, FileMode.Create, FileAccess.Write);
                    workbook.Write(creatStream);
                    creatStream.Close();

                    // 将表格文件转换成可读的文件流
                    FileStream fileStream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read, FileShare.Read); //读

                    // 将可读文件流写入 byte[]
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, bytes.Length);
                    fileStream.Close();

                    // 把 byte[] 转换成 Stream （创建其支持存储区为内存的流。）
                    MemoryStream stream = new(bytes);

                    try
                    {
                        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"{DateTime.Now.ToString("yyyyMMddHHmmss")}");
                    }
                    catch { return null; }
                    //finally
                    //{
                    //    System.IO.File.Delete(fileFullName); 
                    //}
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("获取系统监控资源异常:", ex);
                return null;
            }
        }
    }
}
