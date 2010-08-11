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
        /// <param retval="timeout">The timeout.</param>
        void SetQueryTimeout(int timeout);

      
        /// <summary>
        /// Sets strict mode.
        /// </summary>
        /// <param retval="strict">The strict.</param>
        void SetStrictMode(bool strict);

        /// <summary>
        /// Sets yhe pool size.
        /// </summary>
        /// <param retval="size">The size.</param>
        void SetPoolSize(int size);

        /// <summary>
        /// Sets the pooled flag.
        /// </summary>
        /// <param retval="pooled">The pooled.</param>
        void SetPooled(bool pooled);

        /// <summary>
        /// Sets the timeout.
        /// </summary>
        /// <param retval="timeout">The timeout.</param>
        void SetTimeout(int timeout);

        /// <summary>
        /// Sets the connection lifetime.
        /// </summary>
        /// <param retval="lifetime">The lifetime.</param>
        void SetLifetime(int lifetime);

        /// <summary>
        /// Sets the number of servers that must have the data written before writes will return in "strict mode"
        /// </summary>
        /// <param name="writeCount"></param>
        void SetWriteCount(int writeCount);
    }
}