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
        /// <value></value>
        public double? Uptime { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? localTime { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public GlobalLockResponse GlobalLock { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public MemoryResponse Mem { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public ConnectionsResponse Connections { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public ExtraInfoResponse Extrainfo { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public IndexCountersResponse IndexCounters { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public BackgroundFlushingResponse BackgroundFlushing { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public OpCountersResponse OpCounters { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public AssertsResponse Asserts { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Note { get; private set; }

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
                                                       a.ForProperty(op => op.Note).UseAlias("note");
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
        /// <value></value>
        public double? TotalTime { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? LockTime { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? Ratio { get; private set; }
    }

    /// <summary>
    /// The memory response.
    /// </summary>
    public class MemoryResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Resident { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Virtual { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public long? Mapped { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public bool? Supported { get; private set; }
    }

    /// <summary>
    /// The connections response.
    /// </summary>
    public class ConnectionsResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public long? Current { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public long? Available { get; private set; }
    }

    /// <summary>
    /// The extra info response.
    /// </summary>
    public class ExtraInfoResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Note { get; private set; }
    }

    /// <summary>
    /// The index counters response.
    /// </summary>
    public class IndexCountersResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Note { get; private set; }
    }

    /// <summary>
    /// The background flushing response.
    /// </summary>
    public class BackgroundFlushingResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Flushes { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? TotalMilliseconds { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? AverageMilliseconds { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? LastMilliseconds { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? LastFinished { get; private set; }

        static BackgroundFlushingResponse()
        {
            MongoConfiguration.Initialize(c => c.For<BackgroundFlushingResponse>(a =>
                                                   {
                                                       a.ForProperty(op => op.Flushes).UseAlias("flushes");
                                                       a.ForProperty(op => op.TotalMilliseconds).UseAlias("total_ms");
                                                       a.ForProperty(op => op.AverageMilliseconds).UseAlias("average_ms");
                                                       a.ForProperty(op => op.LastMilliseconds).UseAlias("last_ms");
                                                       a.ForProperty(op => op.LastFinished).UseAlias("last_finished");
                                                   })
                );
        }
    }

    /// <summary>
    /// The opcounters response.
    /// </summary>
    public class OpCountersResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Insertions { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Queries { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Updates { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Deletions { get; private set; }

        /// <summary>How many times a query had to re-query the server for the total resultant set.</summary>
        /// <value></value>
        public int? Pages { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Commands { get; private set; }

        static OpCountersResponse()
        {
            MongoConfiguration.Initialize(c => c.For<OpCountersResponse>(a =>
                                                   {
                                                       a.ForProperty(op => op.Insertions).UseAlias("insert");
                                                       a.ForProperty(op => op.Queries).UseAlias("query");
                                                       a.ForProperty(op => op.Updates).UseAlias("update");
                                                       a.ForProperty(op => op.Deletions).UseAlias("delete");
                                                       a.ForProperty(op => op.Pages).UseAlias("getmore");
                                                       a.ForProperty(op => op.Commands).UseAlias("command");
                                                   })
                );
        }
    }

    /// <summary>
    /// The opcounters response.
    /// </summary>
    public class AssertsResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Regular { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Warning { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Msg { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? User { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public int? Rollovers { get; private set; }
    }
}