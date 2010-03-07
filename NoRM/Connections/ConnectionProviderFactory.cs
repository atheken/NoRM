using System.Collections.Generic;

namespace NoRM
{  
    internal static class ConnectionProviderFactory
    {
        private static volatile IDictionary<string, IConnectionProvider> _providers = new Dictionary<string, IConnectionProvider>();
        private static volatile IDictionary<string, ConnectionStringBuilder> _cachedBuilders = new Dictionary<string, ConnectionStringBuilder>();
        private static readonly object _lock = new object();

        public static IConnectionProvider Create(string connectionString)
        {
            ConnectionStringBuilder builder;
            if (!_cachedBuilders.TryGetValue(connectionString, out builder))
            {
                lock (_lock)
                {
                    if (!_cachedBuilders.TryGetValue(connectionString, out builder))
                    {
                        builder = ConnectionStringBuilder.Create(connectionString);
                        _cachedBuilders.Add(connectionString, builder);
                    }
                }
            }
            
            IConnectionProvider provider;
            if (!_providers.TryGetValue(connectionString, out provider))
            {
                lock(_lock)
                {
                    if (!_providers.TryGetValue(connectionString, out provider))
                    {
                        provider = CreateNewProvider(builder);
                        _providers[connectionString] = provider;
                    }
                }                
            }
            return provider;
        }

        private static IConnectionProvider CreateNewProvider(ConnectionStringBuilder builder)
        {
            if (builder.Pooled)
            {
                return new PooledConnectionProvider(builder);
            }
            return new NormalConnectionProvider(builder);
        }        
    }
}