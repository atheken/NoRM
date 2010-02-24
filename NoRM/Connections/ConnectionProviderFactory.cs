namespace NoRM
{
    using System.Collections.Generic;

    internal static class ConnectionProviderFactory
    {
        private static readonly IDictionary<ConnectionStringBuilder, IConnectionProvider> _providers = new Dictionary<ConnectionStringBuilder, IConnectionProvider>();
        
        public static IConnectionProvider Create(string connectionString)
        {
            IConnectionProvider provider;
            var builder = ConnectionStringBuilder.Create(connectionString);
            if (!_providers.TryGetValue(builder, out provider))
            {
                //todo: consider thread safety
                provider = CreateNewProvider(builder);
                _providers[builder] = provider;
            }
            return provider;
        }

        private static IConnectionProvider CreateNewProvider(ConnectionStringBuilder builder)
        {
            //currently only support pooledconnections
            return new PooledConnectionProvider(builder);
        }
    }
}