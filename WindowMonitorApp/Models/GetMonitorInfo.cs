using System;

namespace WindowMonitorApp.Models
{
    public class GetMonitorInfo
    {
        public double Cpu { get;set; }

        public double Disk { get;set; }

        public double Memo { get;set; } 

        public DateTime DateTime { get;set; }   
    }
}
