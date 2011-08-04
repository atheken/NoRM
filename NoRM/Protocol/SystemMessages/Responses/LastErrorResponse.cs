using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// Indicates what the last error the MongoDB server encountered was.
    /// </summary>
    public class LastErrorResponse : BaseStatusMessage
    {
        static LastErrorResponse()
        {
            MongoConfiguration.Initialize(c => c.For<LastErrorResponse>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.NumberOfUpdates).UseAlias("n");
                                                       a.ForProperty(auth => auth.Error).UseAlias("err");
                                                       a.ForProperty(auth => auth.Code).UseAlias("code");
                                                   })
                );
        }

        /// <summary>
        /// Gets the number of updates.
        /// </summary>
        /// <value>The number of updates.</value>
        public long? NumberOfUpdates{ get; set; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; set; }

        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        public int Code { get; set; }
    }
}