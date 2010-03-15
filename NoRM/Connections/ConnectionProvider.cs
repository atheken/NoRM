using System;
using System.Linq;
using NoRM.Protocol.Messages;
using NoRM.Protocol.SystemMessages.Requests;
using NoRM.Responses;

namespace NoRM
{
    //this base class will eventually serve a purpose

    /// <summary>
    /// Connectin provider
    /// </summary>
    public abstract class ConnectionProvider : IConnectionProvider
    {
        public abstract ConnectionStringBuilder ConnectionString { get; }
        public abstract IConnection Open(string options);
        public abstract void Close(IConnection connection);

        /// <summary>
        /// Creates the new connection.
        /// </summary>
        /// <returns></returns>
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
        /// Authenticates the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        protected bool Authenticate(IConnection connection)
        {
            if (string.IsNullOrEmpty(ConnectionString.UserName))
            {
                return true;
            }

            var nonce = new MongoCollection<GetNonceResponse>("$cmd", new MongoDatabase("admin", connection), connection).FindOne(new { getnonce = 1 });

            if (nonce.Ok == 1)
            {
                var result = new QueryMessage<GenericCommandResponse, AuthenticationRequest>(connection, string.Concat(connection.Database, ".$cmd"))
                {
                    NumberToTake = 1,
                    Query = new AuthenticationRequest
                    {
                        User = connection.UserName,
                        Nonce = nonce.Nonce,
                        Key = connection.Digest(nonce.Nonce),
                    }
                }.Execute();

                return result.Count == 1 && result.Results.ElementAt(0).Ok == 1;
            }

            return false;
        }
    }
}
