using System.Collections.Generic;

namespace Norm
{
    /// <summary>
    /// The connection provider factory.
    /// </summary>
    public static class ConnectionProviderFactory
    {
        private static readonly object _lock = new object();
        private static volatile IDictionary<string, ConnectionOptions> _cachedBuilders = 
            new Dictionary<string, ConnectionOptions>();

        private static volatile IDictionary<string, IConnectionProvider> _providers = 
            new Dictionary<string, IConnectionProvider>();

        /// <summary>
        /// Creates a connection provider.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        /// <returns></returns>
        public static IConnectionProvider Create(string connectionString)
        {
            ConnectionOptions builder;
            if (!_cachedBuilders.TryGetValue(connectionString, out builder))
            {
                lock (_lock)
                {
                    if (!_cachedBuilders.TryGetValue(connectionString, out builder))
                    {
                        builder = ConnectionOptions.Create(connectionString);
                        _cachedBuilders.Add(connectionString, builder);
                    }
                }
            }

            IConnectionProvider provider;
            if (!_providers.TryGetValue(connectionString, out provider))
            {
                lock (_lock)
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

        /// <summary>
        /// The create new provider.
        /// </summary>
        /// <param retval="retval">The retval.</param>
        /// <returns></returns>
        private static IConnectionProvider CreateNewProvider(ConnectionOptions builder)
        {
            if (builder.Pooled)
            {
                return new PooledConnectionProvider(builder);
            }

            return new NormalConnectionProvider(builder);
        }
    }
}