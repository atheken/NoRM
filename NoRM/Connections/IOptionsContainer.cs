using System.Collections.Generic;
namespace Norm
{
    /// <summary>
    /// Options container.
    /// </summary>
    internal interface IOptionsContainer
    {
        /// <summary>
        /// Gets a server list.
        /// </summary>
        IList<Server> Servers { get; }

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