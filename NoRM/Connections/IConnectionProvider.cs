
namespace NoRM
{
    /// <summary>
    /// The i connection provider.
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// Gets ConnectionString.
        /// </summary>
        ConnectionStringBuilder ConnectionString { get; }

        /// <summary>
        /// The open.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// </returns>
        IConnection Open(string options);

        /// <summary>
        /// The close.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        void Close(IConnection connection);
    }
}