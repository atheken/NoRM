
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The profile level response.
    /// </summary>
    public class ProfileLevelResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="ProfileLevelResponse"/> class.
        /// </summary>
        static ProfileLevelResponse()
        {
            MongoConfiguration.Initialize(c => c.For<ProfileLevelResponse>(a =>
                 {
                     a.ForProperty(p => p.PreviousLevel).UseAlias("was");
                     a.ForProperty(p => p.SlowOpThreshold).UseAlias("slowms");
                 }));
        }

        /// <summary>
        /// Gets or sets the previous profiling level.
        /// </summary>
        /// <value>The previous level.</value>
        public int PreviousLevel { get; set; }

        public int? SlowOpThreshold { get; set; }
    }
}