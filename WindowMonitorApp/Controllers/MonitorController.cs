using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowMonitorApp.Commons;
using WindowMonitorApp.Enititys;
using WindowMonitorApp.Models;

namespace WindowMonitorApp.Controllers
{
    [Route("api/Monitor")]
    public class MonitorController : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly MyFreeSql _myFreeSql;

        public MonitorController(ILogger<MonitorController> logger, MyFreeSql myFreeSql)
        {
            this._logger = logger;
            this._myFreeSql = myFreeSql;
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
    }
}
