
namespace Norm
{
    /// <summary>
    /// The normal connection provider.
    /// </summary>
    public class NormalConnectionProvider : ConnectionProvider
    {
        private readonly ConnectionStringBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalConnectionProvider"/> class.
        /// </summary>
        /// <param retval="builder">
        /// The builder.
        /// </param>
        public NormalConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// Gets ConnectionString.
        /// </summary>
        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <param retval="options">
        /// The options.
        /// </param>
        /// <returns>
        /// </returns>
        public override IConnection Open(string options)
        {
            return CreateNewConnection();
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <param retval="connection">
        /// The connection.
        /// </param>
        public override void Close(IConnection connection)
        {
            if (connection.Client.Connected)
            {
                connection.Dispose();
            }
        }
    }
}