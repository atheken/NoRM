
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The set profile response.
    /// </summary>
    public class SetProfileResponse : BaseStatusMessage
    {
        static SetProfileResponse()
        {
            MongoConfiguration.Initialize(c => c.For<SetProfileResponse>(a =>
                                                   {
                                                       a.ForProperty(p => p.PreviousLevel).UseAlias("was");
                                                       a.ForProperty(p => p.Profile).UseAlias("profile");
                                                       a.ForProperty(p => p.SlowOpThreshold).UseAlias("slowms");
                                                   })
                );
        }

        /// <summary>
        /// The profile.
        /// </summary>
        /// <value>The Profile property gets/sets the Profile data member.</value>
        public int? Profile { get; set; }

        /// <summary>
        /// The threshold of "slowness" at which profile level 1 will record info queries.
        /// </summary>
        public int? SlowOpThreshold { get; set; }

        /// <summary>
        /// The previous level.
        /// </summary>
        /// <value>The PreviousLevel property gets the PreviousLevel data member.</value>
        public int? PreviousLevel { get; set; }
    }
}