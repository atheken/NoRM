using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoRM.Responses
{
    public class ServerStatusResponse
    {
        public double? Uptime { get; set; }
        public double? OK { get; set; }
        public GlobalLockResponse GlobalLock { get; set; }
        public MemoryResponse Mem { get; set; }
    }

    public class GlobalLockResponse
    {
        public double? TotalTime { get; set; }
        public double? LockTime { get; set; }
        public double? Ratio { get; set; }
    }

    public class MemoryResponse
    {
        public int? Resident { get; set; }
        public int? Virtual { get; set; }
        public long? Mapped { get; set; }
        public bool? Supported { get; set; }
    }
}
