using Norm.Configuration;

namespace Norm.Protocol.SystemMessages.Requests
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
        /// <value>The Authenticate property gets the Authenticate data member.</value>
        public int Authenticate
        {
            get { return 1; }
        }

        /// <summary>
        /// The nonce.
        /// </summary>
        /// <value>The Nonce property gets/sets the Nonce data member.</value>
        public string Nonce { get; set; }

        /// <summary>
        /// The user.
        /// </summary>
        /// <value>The User property gets/sets the User data member.</value>
        public string User { get; set; }

        /// <summary>
        /// The fieldSelectionExpando.
        /// </summary>
        /// <value>The Key property gets/sets the Key data member.</value>
        public string Key { get; set; }
    }
}