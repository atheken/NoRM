namespace NoRM
{
    public class NormalConnectionProvider : ConnectionProvider
    {
        private readonly ConnectionStringBuilder _builder;
        
        public override ConnectionStringBuilder ConnectionString
        {
            get { return _builder; }
        }
        public NormalConnectionProvider(ConnectionStringBuilder builder)
        {
            _builder = builder;            
        }
        
        public override IConnection Open(string options)
        {
            return CreateNewConnection();
        }

        public override void Close(IConnection connection)
        {
            if (connection.Client.Connected)
            {
                connection.Dispose();
            }
        }
    }
}