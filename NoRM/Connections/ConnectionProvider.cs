using System;
using System.Linq;
using NoRM.Protocol.Messages;
using NoRM.Protocol.SystemMessages.Requests;
using NoRM.Responses;

namespace NoRM.Connections
{
    // this base class will eventually serve a purpose

    /// <summary>
    /// The connection provider.
    /// </summary>
    public abstract class ConnectionProvider : IConnectionProvider
    {
        /// <summary>
        /// Gets a connection string.
        /// </summary>
        public abstract ConnectionStringBuilder ConnectionString { get; }

        /// <summary>
        /// Opens a connection
        /// </summary>
        /// <param name="options">connection options.</param>
        /// <returns></returns>
        public abstract IConnection Open(string options);

        /// <summary>
        /// Closes a connection.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public abstract void Close(IConnection connection);

        /// <summary>
        /// The create new connection.
        /// </summary>
        /// <returns>
        /// </returns>
        protected IConnection CreateNewConnection()
        {
            var connection = new Connection(ConnectionString);
            try
            {
                if (!Authenticate(connection))
                {
                    Close(connection);
                }
            }
            catch (Exception)
            {
                Close(connection);
                throw;
            }

            return connection;
        }

        /// <summary>
        /// Athenticates a connection.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <returns>
        /// Authenticated.
        /// </returns>
        protected bool Authenticate(IConnection connection)
        {
            if (string.IsNullOrEmpty(ConnectionString.UserName))
            {
                return true;
            }

            var nonce = new MongoCollection<GetNonceResponse>("$cmd", new MongoDatabase("admin", connection), connection).FindOne(new { getnonce = 1 });

            if (nonce.OK == 1)
            {
                var result = new QueryMessage<GenericCommandResponse, AuthenticationRequest>(connection, string.Concat(connection.Database, ".$cmd"))
                    {
                        NumberToTake = 1,
                        Query = new AuthenticationRequest
                        {
                            user = connection.UserName,
                            nonce = nonce.Nonce,
                            key = connection.Digest(nonce.Nonce),
                        }
                    }
                    .Execute();

                return result.Count == 1 && result.Results.ElementAt(0).OK == 1;
            }

            return false;
        }
    }
}
