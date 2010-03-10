namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// The authentication request.
    /// </summary>
    internal class AuthenticationRequest
    {
        /// <summary>
        /// Authenticate.
        /// </summary>
        public int authenticate
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        public string nonce { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string user { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string key { get; set; }
    }
}