using Norm.Configuration;
using Norm.BSON;

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
        public double? localTime { get; set; }

        /// <summary>TODO::Description.</summary>
        public GlobalLockResponse GlobalLock { get; set; }

        /// <summary>TODO::Description.</summary>
        public MemoryResponse Mem { get; set; }

        /// <summary>TODO::Description.</summary>
        public ConnectionsResponse Connections { get; set; }

        /// <summary>TODO::Description.</summary>
        public ExtraInfoResponse Extrainfo { get; set; }

        /// <summary>TODO::Description.</summary>
        public IndexCountersResponse IndexCounters { get; set; }

        /// <summary>TODO::Description.</summary>
        public BackgroundFlushingResponse BackgroundFlushing { get; set; }

        /// <summary>TODO::Description.</summary>
        public OpCountersResponse OpCounters { get; set; }

        /// <summary>TODO::Description.</summary>
        public AssertsResponse Asserts { get; set; }

        static ServerStatusResponse()
        {
            MongoConfiguration.Initialize(c => c.For<ServerStatusResponse>(a =>
                                                   {
                                                       a.ForProperty(op => op.GlobalLock).UseAlias("globalLock");
                                                       a.ForProperty(op => op.Mem).UseAlias("mem");
                                                       a.ForProperty(op => op.Connections).UseAlias("connections");
                                                       a.ForProperty(op => op.Extrainfo).UseAlias("extra_info");
                                                       a.ForProperty(op => op.IndexCounters).UseAlias("indexCounters");
                                                       a.ForProperty(op => op.BackgroundFlushing).UseAlias("backgroundFlushing");
                                                       a.ForProperty(op => op.OpCounters).UseAlias("opcounters");
                                                       a.ForProperty(op => op.Asserts).UseAlias("asserts");
                                                   })
                );
        }
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

    /// <summary>
    /// The connections response.
    /// </summary>
    public class ConnectionsResponse
    {
        /// <summary>TODO::Description.</summary>
        public long? current { get; set; }

        /// <summary>TODO::Description.</summary>
        public long? available { get; set; }
    }

    /// <summary>
    /// The extra info response.
    /// </summary>
    public class ExtraInfoResponse
    {
        /// <summary>TODO::Description.</summary>
        public string note { get; set; }
    }

    /// <summary>
    /// The index counters response.
    /// </summary>
    public class IndexCountersResponse
    {
        /// <summary>TODO::Description.</summary>
        public string note { get; set; }
    }

    /// <summary>
    /// The background flushing response.
    /// </summary>
    public class BackgroundFlushingResponse
    {
        /// <summary>TODO::Description.</summary>
        public int? flushes { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? total_ms { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? average_ms { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? last_ms { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? last_finished { get; set; }
    }

    /// <summary>
    /// The opcounters response.
    /// </summary>
    public class OpCountersResponse
    {
        /// <summary>TODO::Description.</summary>
        public int? insert { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? query { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? update { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? delete { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? getmore { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? command { get; set; }
    }

    /// <summary>
    /// The opcounters response.
    /// </summary>
    public class AssertsResponse
    {
        /// <summary>TODO::Description.</summary>
        public int? regular { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? warning { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? msg { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? user { get; set; }

        /// <summary>TODO::Description.</summary>
        public int? rollovers { get; set; }
    }

    
}