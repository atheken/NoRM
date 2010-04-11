using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// Represents a message with an Ok status
    /// </summary>
    public class BaseStatusMessage
    {
        static BaseStatusMessage()
        {
            MongoConfiguration.Initialize(c => c.For<BaseStatusMessage>(a =>
                       {
                           a.ForProperty(auth => auth.Ok).UseAlias("ok");
                       })
                );
        }

        /// <summary>
        /// Gets or sets the operation status.
        /// </summary>
        /// <value>The operation status.</value>
        public double? Ok { get; set; }
    }
}
