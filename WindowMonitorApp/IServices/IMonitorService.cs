using System.Threading.Tasks;

namespace WindowMonitorApp.IServices
{
    public interface IMonitorService
    {
        void StartMonitor();

        Task DeleteHistoryInfo();
    }
}
