namespace NoRM
{
    using System.Net.Sockets;
    
    public interface IConnection
    {
        TcpClient Client{ get;}
        NetworkStream GetStream();
        int QueryTimeout{ get;}       
        bool EnableExpandoProperties{ get;}        
    }
        
    //todo: cleanup, timeout, age hanlding
    public class Connection : IConnection    
    {        
        public TcpClient Client{ get; private set;}
        private readonly ConnectionStringBuilder _builder;      
        
        public int QueryTimeout
        {
            get { return _builder.QueryTimeout; }
        }
        public bool EnableExpandoProperties
        {
            get { return _builder.EnableExpandoProperties; }
        }
        
        internal Connection(ConnectionStringBuilder builder)
        {
            _builder = builder;
            Client = new TcpClient();
            Client.Connect(builder.Servers[0].Host, builder.Servers[0].Port);            
        }

        public NetworkStream GetStream()
        {
            return Client.GetStream();
        }

    }
}