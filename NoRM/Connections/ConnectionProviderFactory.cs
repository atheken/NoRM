using System.Collections.Generic;

namespace Norm
{
    /// <summary>
    /// The connection provider factory.
    /// </summary>
    internal static class ConnectionProviderFactory
    {
        private static readonly object _lock = new object();
        private static volatile IDictionary<string, ConnectionStringBuilder> _cachedBuilders = 
            new Dictionary<string, ConnectionStringBuilder>();

        private static volatile IDictionary<string, IConnectionProvider> _providers = 
            new Dictionary<string, IConnectionProvider>();

        /// <summary>
        /// Creates a connection provider.
        /// </summary>
        /// <param retval="connectionString">The connection string.</param>
        /// <returns></returns>
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
        /// <param retval="builder">The builder.</param>
        /// <returns></returns>
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