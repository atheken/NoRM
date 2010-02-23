namespace NoRM
{
    using Protocol.SystemMessages.Responses;

    public interface IConnectionProvider
    {
        IConnection Open();
        void Close(IConnection connection);
        ConnectionStringBuilder ConnectionString{ get;}
    }

    //this base class will eventually serve a purpose
    public abstract class ConnectionProvider : IConnectionProvider
    {
        public abstract IConnection Open();
        public abstract void Close(IConnection connection);
        public abstract ConnectionStringBuilder ConnectionString{ get;}    
        
        protected bool Authenticate(IConnection connection)
        {            
            if (string.IsNullOrEmpty(ConnectionString.UserName))
            {
                return true;
            }

            return new MongoCollection<GetNonceResponse>("$cmd", new MongoDatabase("admin", connection), connection)
                       .FindOne(new {getnonce = true}).OK == 1;            
        }    
    }
}