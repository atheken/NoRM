namespace Norm
{
    /// <summary>
    /// Options container.
    /// </summary>
    internal interface IOptionsContainer
    {
        /// <summary>
        /// Sets the query timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void SetQueryTimeout(int timeout);

      
        /// <summary>
        /// Sets strict mode.
        /// </summary>
        /// <param name="strict">The strict.</param>
        void SetStrictMode(bool strict);

        /// <summary>
        /// Sets yhe pool size.
        /// </summary>
        /// <param name="size">The size.</param>
        void SetPoolSize(int size);

        /// <summary>
        /// Sets the pooled flag.
        /// </summary>
        /// <param name="pooled">The pooled.</param>
        void SetPooled(bool pooled);

        /// <summary>
        /// Sets the timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void SetTimeout(int timeout);

        /// <summary>
        /// Sets the connection lifetime.
        /// </summary>
        /// <param name="lifetime">The lifetime.</param>
        void SetLifetime(int lifetime);
    }
}