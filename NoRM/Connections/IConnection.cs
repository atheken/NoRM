using System;
using System.Net.Sockets;

namespace Norm
{
    /// <summary>
    /// A database connection
    /// </summary>
    public interface IConnection : IDisposable
    {
        /// <summary>
        /// Get the write count required to be returned from the server when strict mode is enabled.
        /// </summary>
        int VerifyWriteCount { get; }

        /// <summary>
        /// The connection string used to create this connection.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the tcp client.
        /// </summary>
        TcpClient Client { get; }

        /// <summary>
        /// Gets a value indicating whether the client is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets a value indicating whether the connection is invalid.
        /// </summary>
        bool IsInvalid { get; }

        /// <summary>
        /// Gets the connection created date.
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Gets the query timeout.
        /// </summary>
        int QueryTimeout { get; }


        /// <summary>
        /// Gets a value indicating whether to use strict mode.
        /// </summary>
        bool StrictMode { get; }

        /// <summary>
        /// Gets the user retval.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets the database retval.
        /// </summary>
        string Database { get; }

        /// <summary>
        /// Gets a stream.
        /// </summary>
        /// <returns></returns>
        NetworkStream GetStream();

        /// <summary>
        /// The digest.
        /// </summary>
        /// <param retval="nounce">The nounce.</param>
        /// <returns>The digest.</returns>
        string Digest(string nounce);

        /// <summary>
        /// Loads options.
        /// </summary>
        /// <param retval="options">The options.</param>
        void LoadOptions(string options);

        /// <summary>
        /// Resets options.
        /// </summary>
        void ResetOptions();

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param retval="bytes">The bytes.</param>
        /// <param retval="start">The start.</param>
        /// <param retval="size">The size.</param>
        void Write(byte[] bytes, int start, int size);
    }
}