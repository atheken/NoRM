namespace NoRM
{
    using System.Collections.Generic;

    internal static class ConnectionProviderFactory
    {
        private static readonly IDictionary<string, IConnectionProvider> _providers = new Dictionary<string, IConnectionProvider>();
        
        public static IConnectionProvider Create(string connectionString)
        {
            IConnectionProvider provider;
            if (!_providers.TryGetValue(connectionString, out provider))
            {
                //todo: consider thread safety
                provider = CreateNewProvider(connectionString);
                _providers[connectionString] = provider;
            }
            return provider;
        }

        private static IConnectionProvider CreateNewProvider(string connectionString)
        {
            //currently only support pooledconnections
            return new PooledConnectionProvider(ConnectionStringBuilder.Create(connectionString));
        }
    }
}