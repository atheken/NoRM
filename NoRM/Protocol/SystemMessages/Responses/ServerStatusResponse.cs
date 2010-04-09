
namespace Norm.Responses
{
    /// <summary>
    /// The server status response.
    /// </summary>
    public class ServerStatusResponse : BaseStatusMessage
    {
        public double? Uptime { get; set; }
        public GlobalLockResponse GlobalLock { get; set; }
        public MemoryResponse Mem { get; set; }
    }

    /// <summary>
    /// The global lock response.
    /// </summary>
    public class GlobalLockResponse
    {
        public double? TotalTime { get; set; }
        public double? LockTime { get; set; }
        public double? Ratio { get; set; }
    }

    /// <summary>
    /// The memory response.
    /// </summary>
    public class MemoryResponse
    {
        public int? Resident { get; set; }
        public int? Virtual { get; set; }
        public long? Mapped { get; set; }
        public bool? Supported { get; set; }
    }
}