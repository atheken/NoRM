
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// Indicates the result of a demand that MongoDB flush non-file committed writes to their respective files.
    /// </summary>
    public class ForceSyncResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="ForceSyncResponse"/> class.
        /// </summary>
        static ForceSyncResponse()
        {
            MongoConfiguration.Initialize(c => c.For<ForceSyncResponse>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.NumberOfFiles).UseAlias("numFiles");
                                                   })
                );
        }

        /// <summary>
        /// Gets the number of files.
        /// </summary>
        /// <value>The number of files.</value>
        public int? NumberOfFiles { get; set; }
    }
}
