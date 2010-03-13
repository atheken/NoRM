using NoRM.Configuration;

namespace NoRM.Protocol.SystemMessages.Requests
{
    /// <summary>
    /// The authentication request.
    /// </summary>
    internal class AuthenticationRequest : ISystemQuery
    {
        /// <summary>
        /// Initializes the <see cref="AuthenticationRequest"/> class.
        /// </summary>
        static AuthenticationRequest()
        {
            MongoConfiguration.Initialize(c => c.For<AuthenticationRequest>(a =>
                                               {
                                                   a.ForProperty(auth => auth.Authenticate).UseAlias("authenticate");
                                                   a.ForProperty(auth => auth.Nonce).UseAlias("nonce");
                                                   a.ForProperty(auth => auth.User).UseAlias("user");
                                                   a.ForProperty(auth => auth.Key).UseAlias("key");
                                               })
                                        );
        }

        /// <summary>
        /// Authenticate.
        /// </summary>
        public int Authenticate
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the nonce.
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }
    }
}