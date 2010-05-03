
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
                 { a.ForProperty(auth => auth.PreviousLevel).UseAlias("was"); }));
        }

        /// <summary>
        /// Gets or sets the previous profiling level.
        /// </summary>
        /// <value>The previous level.</value>
        public int PreviousLevel { get; set; }
    }
}