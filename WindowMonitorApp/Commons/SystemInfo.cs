using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Management;
using System.Diagnostics;
using System.Security.Cryptography;
using WindowMonitorApp.Models;
using System.Threading;
using WindowMonitorApp.Enititys;
using Serilog;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Hangfire.Logging;

namespace WindowMoniterApp.Commons
{
    public class SystemInfo
    {
        private int m_ProcessorCount = 0;   //CPU个数
        private PerformanceCounter pcCpuLoad;   //CPU计数器
        private long m_PhysicalMemory = 0;   //物理内存
        private static DateTime lastTime;
        private static TimeSpan lastTotalProcessorTime;
        private static DateTime curTime;
        private static TimeSpan curTotalProcessorTime;
        public bool bOn = true;
        public readonly PerformanceCounter totalCPU = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");

        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;

        #region AIP声明
        [DllImport("IpHlpApi.dll")]
        extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数，初始化计数器等
        /// </summary>
        public SystemInfo()
        {
            ////启动CPU使用率刷新线程
            //Task.Factory.StartNew(() =>
            //{
            //    CPUUsageProc();
            //},
            //TaskCreationOptions.LongRunning);
        }
        #endregion

        ManagementClass mc = new ManagementClass("Win32_ComputerSystem");

        public void GetMemory()
        {
            //获得物理内存
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    m_PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
            moc.Dispose();

        }
        PerformanceCounterCategory counterCategory = new PerformanceCounterCategory("PhysicalDisk");

        public List<DiskDetailInfo> GetComputerDiskRunInfo()
        {

            string[] instanceNames = counterCategory.GetInstanceNames();
            List<DiskDetailInfo> diskDetailInfos = new List<DiskDetailInfo>();
            string s;
            string d;
            foreach (string instanceName in instanceNames)
            {
                DiskDetailInfo diskDetailInfo = new DiskDetailInfo();
                diskDetailInfo.DiskInfo = instanceName;

               // Console.Write("实例名：{0}", instanceName);
                diskDetailInfo.ReadLoad = Math.Round(GetDiskData(DiskAccessType.Read, instanceName),2);
                diskDetailInfo.ReadLoadInfo = FormatBytes(diskDetailInfo.ReadLoad) + "/s";
                //s = "\r\n\t硬盘读取速率 (" + FormatBytes(diskDetailInfo.ReadLoad) + "/s)";
                //Console.WriteLine(s);
                diskDetailInfo.WriteLoad = Math.Round(GetDiskData(DiskAccessType.Write, instanceName), 2);
                diskDetailInfo.WriteLoadInfo = FormatBytes(diskDetailInfo.WriteLoad) + "/s";
                //d = GetDiskData(DiskAccessType.Write, instanceName);
                //s = "\t硬盘写入速率 (" + FormatBytes(diskDetailInfo.WriteLoad) + "/s)";
                //Console.WriteLine(s);

                diskDetailInfo.DiskLoad = Math.Round(GetDiskData(DiskAccessType.DiskTime, instanceName),2);
                diskDetailInfo.DiskLoadInfo = diskDetailInfo.DiskLoad.ToString("f4") + " %";
                //d = GetDiskData(DiskAccessType.DiskTime, instanceName);
                //s = "\t硬盘IO百分比 (" + diskDetailInfo.DiskLoad.ToString("f4") + " %)";
                //Console.WriteLine(s);
                diskDetailInfos.Add(diskDetailInfo);
            }

            return diskDetailInfos;
        }
        #region CPU个数
        /// <summary>
        /// 获取CPU个数
        /// </summary>
        public int ProcessorCount
        {
            get
            {
                return m_ProcessorCount;
            }
        }
        #endregion

        #region CPU占用率
        /// <summary>
        /// 获取CPU占用率
        /// </summary>
        public float CpuLoad
        {
            get
            {
                return pcCpuLoad.NextValue();
            }
        }

        public float GetCpu()
        {
            float cpu = 0;
            float cpu1 = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select PercentProcessorTime from Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
            var cpuItem = searcher.Get().Cast<ManagementObject>().Select(item => new { PercentProcessorTime = item["PercentProcessorTime"] }).First();
            if (cpuItem != null && cpuItem.PercentProcessorTime != null)
            {
                if (float.TryParse(cpuItem.PercentProcessorTime.ToString(), out cpu1))
                {
                    cpu = cpu1;
                }
            }
            return cpu;

        }

        public int cpuUsage;

        //private void CPUUsageProc(int cpuLimit)
        //{
        //    while (bOn)
        //    {
        //        try
        //        {
        //            int cpuUsage = (int)totalCPU.NextValue();
        //            if (cpuUsage > cpuLimit)
        //            {
        //                //log.Warn(GlobalData.PGName, $"CPU 整体使用率：{cpuUsage}%");
        //                for (int i = 0; i < Constant.procNames.Length; i++)
        //                {
        //                    Process[] pp = Process.GetProcessesByName(Constant.procNames[i]);
        //                    if (pp.Length == 0)
        //                    {
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        Process p = pp[0];
        //                        if (lastTime == null || lastTime == new DateTime())
        //                        {
        //                            lastTime = DateTime.Now;
        //                            lastTotalProcessorTime = p.TotalProcessorTime;
        //                        }
        //                        else
        //                        {
        //                            curTime = DateTime.Now;
        //                            curTotalProcessorTime = p.TotalProcessorTime;

        //                            double CPUUsage = (curTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / curTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
        //                            //log.Warn(GlobalData.PGName, $"进程{Constant.procNames[i]} CPU 使用率：{CPUUsage * 100:0.0}%");

        //                            lastTime = curTime;
        //                            lastTotalProcessorTime = curTotalProcessorTime;
        //                        }
        //                    }
        //                }
        //            }
        //            Thread.Sleep(500);
        //        }
        //        catch (Exception ex)
        //        {
        //            //log.Error(GlobalData.PGName, "CPU使用率刷新异常," + ex);
        //            Thread.Sleep(1000);
        //        }
        //    }
        //}
        #endregion

        #region 可用内存
        /// <summary>
        /// 获取可用内存
        /// </summary>

        ManagementClass mos = new ManagementClass("Win32_OperatingSystem");

        public long MemoryAvailable
        {
            get
            {
                long availablebytes = 0;

                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
        }
        #endregion

        #region 物理内存
        /// <summary>
        /// 获取物理内存
        /// </summary>
        public long PhysicalMemory
        {
            get
            {
                return m_PhysicalMemory;
            }
        }
        #endregion

        #region 结束指定进程
        /// <summary>
        /// 结束指定进程
        /// </summary>
        /// <param name="pid">进程的 Process ID</param>
        public static void EndProcess(int pid)
        {
            try
            {
                Process process = Process.GetProcessById(pid);
                process.Kill();
            }
            catch { }
        }
        #endregion


        #region 查找所有应用程序标题
        /// <summary>
        /// 查找所有应用程序标题
        /// </summary>
        /// <returns>应用程序标题范型</returns>
        public static List<string> FindAllApps(int Handle)
        {
            List<string> Apps = new List<string>();

            int hwCurr;
            hwCurr = GetWindow(Handle, GW_HWNDFIRST);

            while (hwCurr > 0)
            {
                int IsTask = (WS_VISIBLE | WS_BORDER);
                int lngStyle = GetWindowLongA(hwCurr, GWL_STYLE);
                bool TaskWindow = ((lngStyle & IsTask) == IsTask);
                if (TaskWindow)
                {
                    int length = GetWindowTextLength(new IntPtr(hwCurr));
                    StringBuilder sb = new StringBuilder(2 * length + 1);
                    GetWindowText(hwCurr, sb, sb.Capacity);
                    string strTitle = sb.ToString();
                    if (!string.IsNullOrEmpty(strTitle))
                    {
                        Apps.Add(strTitle);
                    }
                }
                hwCurr = GetWindow(hwCurr, GW_HWNDNEXT);
            }

            return Apps;
        }


        #region 磁盘信息


        public enum DiskAccessType { ReadAndWrite, Read, Write, DiskTime };
        static PerformanceCounter _diskReadCounter = new PerformanceCounter();
        static PerformanceCounter _diskWriteCounter = new PerformanceCounter();
        static PerformanceCounter _diskTimeWriteCounter = new PerformanceCounter();

        public static double GetDiskData(DiskAccessType dd, string instanceName)
        {
            //有数据
            string _instanceName = "_Total";
            //无数据
            //string _instanceName = instanceName;

            double r = 0;
            switch (dd)
            {
                case DiskAccessType.Read:
                    r = GetCounterValue(_diskReadCounter, "PhysicalDisk", "Disk Read Bytes/sec", _instanceName);
                    break;
                case DiskAccessType.Write:
                    r = GetCounterValue(_diskWriteCounter, "PhysicalDisk", "Disk Write Bytes/sec", _instanceName);
                    break;
                case DiskAccessType.ReadAndWrite:
                    r = GetCounterValue(_diskReadCounter, "PhysicalDisk", "Disk Read Bytes/sec", _instanceName) +
                        GetCounterValue(_diskWriteCounter, "PhysicalDisk", "Disk Write Bytes/sec", _instanceName);
                    break;
                case DiskAccessType.DiskTime:
                    r = GetCounterValue(_diskTimeWriteCounter, "PhysicalDisk", "% Disk Time", _instanceName);
                    break;
            }

            return r;
        }

        static double GetCounterValue(PerformanceCounter pc, string categoryName, string counterName, string instanceName)
        {
            pc.CategoryName = categoryName;
            pc.CounterName = counterName;
            pc.InstanceName = instanceName;
            return pc.NextValue();
        }

        public static string FormatBytes(double bytes)
        {
            string s = (bytes / (1024 * 1024.0)).ToString("f4") + " MB";
            return s;
        }

        #endregion


        #endregion
    }
}

