using System.Collections.Generic;
using Norm.Protocol;
namespace Norm
{
    /// <summary>
    /// Options container.
    /// </summary>
    internal interface IOptionsContainer
    {

        /// <summary>
        /// Indicates that any server (primary or secondary) is safe to read from -- only useful when
        /// "UseReplicaSets" is enabled, and there are secondary servers.
        /// </summary>
        bool ReadFromAny { get; }

        /// <summary>
        /// Indicates if this the options specified here should be used in the context of a Replica Set
        /// (this has many implications, including detecting additional member servers, automatic failover,
        /// reconnects, arbiters, a big ball of wax.)
        /// </summary>
        bool UseReplicaSets { get; }

        /// <summary>
        /// Gets a server list.
        /// </summary>
        IList<ClusterMember> Servers { get; }

        /// <summary>
        /// Gets the user retval.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets database retval.
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Gets the query timeout.
        /// </summary>
        int QueryTimeout { get; }

        /// <summary>
        /// Gets a value indicating whether strict mode is enabled.
        /// </summary>
        bool StrictMode { get; }

        /// <summary>
        /// Gets a value indicating whether connections are pooled.
        /// </summary>
        bool Pooled { get; }

        /// <summary>
        /// Gets the connection pool size.
        /// </summary>
        int PoolSize { get; }

        /// <summary>
        /// Gets the connection timeout.
        /// </summary>
        int Timeout { get; }

        /// <summary>
        /// Gets the connection lifetime.
        /// </summary>
        int Lifetime { get; }

        /// <summary>
        /// Get the write count required to be returned from the server when strict mode is enabled.
        /// </summary>
        int? VerifyWriteCount { get; }
    }
}