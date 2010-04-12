
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The generic command response.
    /// </summary>
    public class GenericCommandResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="GenericCommandResponse"/> class.
        /// </summary>
        static GenericCommandResponse()
        {
            MongoConfiguration.Initialize(c => c.For<GenericCommandResponse>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.Info).UseAlias("info");
                                                   })
                );
        }

        /// <summary>
        /// Gets the command info.
        /// </summary>
        /// <value>The info.</value>
        public string Info { get; set; }
    }
}