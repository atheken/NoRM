
namespace Norm.Responses
{
    /// <summary>
    /// The server status response.
    /// </summary>
    public class ServerStatusResponse : BaseStatusMessage
    {
        /// <summary>TODO::Description.</summary>
        public double? Uptime { get; set; }

        /// <summary>TODO::Description.</summary>
        public GlobalLockResponse GlobalLock { get; set; }

        /// <summary>TODO::Description.</summary>
        public MemoryResponse Mem { get; set; }
    }

    /// <summary>
    /// The global lock response.
    /// </summary>
    public class GlobalLockResponse
    {
        /// <summary>TODO::Description.</summary>
        public double? TotalTime { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? LockTime { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? Ratio { get; set; }
    }

    /// <summary>
    /// The memory response.
    /// </summary>
    public class MemoryResponse
    {
        /// <summary>TODO::Description.</summary>
        public int? Resident { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? Virtual { get; set; }

        /// <summary>TODO::Description.</summary>
        public long? Mapped { get; set; }

        /// <summary>TODO::Description.</summary>
        public bool? Supported { get; set; }
    }
}