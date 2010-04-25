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
        /// The operation status
        /// </summary>
        /// <value>The Ok property gets the Ok data member.</value>
        public double? Ok { get; set; }
    }
}
