using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowMonitorApp.Commons;
using WindowMonitorApp.Enititys;

namespace WindowMonitorApp.Controllers
{
    public class MonitorController : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly MyFreeSql _myFreeSql;

        public MonitorController(ILogger<MonitorController> logger, MyFreeSql myFreeSql)
        {
            this._logger = logger;
            this._myFreeSql = myFreeSql;
        }

        [Route("GetWindowMonitor")]
        public async Task<List<ComputerRunInfo>> GetWindowMonitorAsync()
        {
            var qeury =await _myFreeSql._freeSql.GetRepository<ComputerRunInfo>().Select.ToListAsync();
            return qeury;
        }
    }
}
