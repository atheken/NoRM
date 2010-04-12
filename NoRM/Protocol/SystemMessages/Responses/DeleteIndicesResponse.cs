
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The delete indices response.
    /// </summary>
    internal class DeleteIndicesResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="DeleteIndicesResponse"/> class.
        /// </summary>
        static DeleteIndicesResponse()
        {
            MongoConfiguration.Initialize(c => c.For<DeleteIndicesResponse>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.NumberIndexesWas).UseAlias("nIndexesWas");
                                                       a.ForProperty(auth => auth.Message).UseAlias("msg");
                                                       a.ForProperty(auth => auth.Namespace).UseAlias("ns ");
                                                   })
                );
        }

        /// <summary>
        /// Gets or sets the number of previous indexes.
        /// </summary>
        /// <value>The number indexes was.</value>
        public int? NumberIndexesWas { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public string Namespace { get; set; }
    }
}
