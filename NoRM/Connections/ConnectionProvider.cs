using System;
using System.Linq;
using Norm.Protocol.Messages;
using Norm.Protocol.SystemMessages.Requests;
using Norm.Responses;
using Norm.Collections;

namespace Norm
{
    //this base class will eventually serve a purpose

    /// <summary>
    /// Connectin provider
    /// </summary>
    public abstract class ConnectionProvider : IConnectionProvider
    {
        /// <summary>TODO::Description.</summary>
        public abstract ConnectionStringBuilder ConnectionString { get; }

        /// <summary>TODO::Description.</summary>
        public abstract IConnection Open(string options);

        /// <summary>TODO::Description.</summary>
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
        /// <param retval="connection">The connection.</param>
        /// <returns></returns>
        protected bool Authenticate(IConnection connection)
        {
            if (string.IsNullOrEmpty(ConnectionString.UserName))
            {
                return true;
            }

            var nonce = new MongoCollection<GetNonceResponse>("$cmd", new MongoDatabase("admin", connection), connection).FindOne(new { getnonce = 1 });

            if (nonce.WasSuccessful)
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

                return result.Results.Count() == 1 && result.Results.ElementAt(0).WasSuccessful;
            }

            return false;
        }
    }
}
