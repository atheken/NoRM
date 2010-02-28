namespace NoRM
{
    using System;
    using Protocol.Messages;
    using Protocol.SystemMessages.Requests;
    using Protocol.SystemMessages.Responses;
    using System.Linq;

    public interface IConnectionProvider
    {
        IConnection Open(string options);
        void Close(IConnection connection);
        ConnectionStringBuilder ConnectionString{ get;}
    }

    //this base class will eventually serve a purpose
    public abstract class ConnectionProvider : IConnectionProvider
    {
        public abstract IConnection Open(string options);
        public abstract void Close(IConnection connection);
        public abstract ConnectionStringBuilder ConnectionString{ get;}

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
          
        protected bool Authenticate(IConnection connection)
        {            
            if (string.IsNullOrEmpty(ConnectionString.UserName))
            {
                return true;
            }

            var nonce = new MongoCollection<GetNonceResponse>("$cmd", new MongoDatabase("admin", connection), connection).FindOne(new {getnonce = 1});

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
                    }.Execute();

                return result.Count == 1 && result.Results.ElementAt(0).OK == 1;                
            }

            return false;
        }
    }
}