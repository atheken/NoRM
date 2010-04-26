
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The get nonce response.
    /// </summary>
    internal class GetNonceResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="GetNonceResponse"/> class.
        /// </summary>
        static GetNonceResponse()
        {
            MongoConfiguration.Initialize(c => c.For<GetNonceResponse>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.Nonce).UseAlias("nonce");
                                                   })
                );
        }

        /// <summary>
        /// Gets the nonce.
        /// </summary>
        /// <value>The nonce.</value>
        public string Nonce { get; set; }
    }
}