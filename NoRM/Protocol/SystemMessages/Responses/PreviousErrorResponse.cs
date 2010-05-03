
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The previous error response.
    /// </summary>
    public class PreviousErrorResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="PreviousErrorResponse"/> class.
        /// </summary>
        static PreviousErrorResponse()
        {
            MongoConfiguration.Initialize(c => c.For<PreviousErrorResponse>(a =>
                 {
                     a.ForProperty(auth => auth.NumberOfErrors).UseAlias("n");
                     a.ForProperty(auth => auth.Error).UseAlias("err");
                     a.ForProperty(auth => auth.NumberOfOperationsAgo).UseAlias("nPrev");
                 })
                );
        }

        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        /// <value>The number of errors.</value>
        public long? NumberOfErrors { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the number of operations ago.
        /// </summary>
        /// <value>The number of operations ago.</value>
        public long? NumberOfOperationsAgo { get; set; }
    }
}