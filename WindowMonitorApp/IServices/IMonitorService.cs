using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowMonitorApp.Enititys;

namespace WindowMonitorApp.IServices
{
    public interface IMonitorService
    {

        /// <summary>
        /// 数据监控
        /// </summary>
        void StartMonitor();

        /// <summary>
        /// 删除历史数据
        /// </summary>
        /// <returns></returns>
        Task DeleteHistoryInfo();

        /// <summary>
        /// 导出监控数据
        /// </summary>
        /// <param name="computerRunInfos"></param>
        /// <returns></returns>
        Task<IWorkbook> ExportMonitorInfoAsync(List<ComputerRunInfo> computerRunInfos);
       
    }
}
