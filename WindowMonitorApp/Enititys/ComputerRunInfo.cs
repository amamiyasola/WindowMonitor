using System;
using System.Collections.Generic;

namespace WindowMonitorApp.Enititys
{
    public class ComputerRunInfo
    {

        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Cpu占用率信息
        /// </summary>
        public string CpuLoadInfo { get; set; }

        /// <summary>
        /// Cpu占用率
        /// </summary>
        public double CpuLoad { get; set; }



        /// <summary>
        /// 内存占用率信息
        /// </summary>
        public string MemLoadInfo { get; set; }


        /// <summary>
        /// 内存占用率
        /// </summary>
        public double MemLoad { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// 磁盘占比信息
        /// </summary>
        public string DiskDetailInfos { get; set; }

    }

    /// <summary>
    /// 磁盘信息
    /// </summary>
    public class DiskDetailInfo
    {

        /// <summary>
        /// 磁盘占用率信息
        /// </summary>
        public string DiskLoadInfo { get; set; }


        /// <summary>
        /// 磁盘占用率
        /// </summary>
        public double DiskLoad { get; set; }

        /// <summary>
        /// 写入率
        /// </summary>
        public double WriteLoad { get; set; }

        /// <summary>
        /// 写入信息
        /// </summary>
        public string WriteLoadInfo { get; set; }


        /// <summary>
        /// 读取率
        /// </summary>
        public double ReadLoad { get; set; }

        /// <summary>
        /// 读取率信息
        /// </summary>
        public string ReadLoadInfo { get; set; }

        /// <summary>
        /// 磁盘信息
        /// </summary>
        public string DiskInfo { get; set; }
    }
}
