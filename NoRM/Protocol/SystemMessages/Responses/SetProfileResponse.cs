
using NoRM.Configuration;

namespace NoRM.Responses
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
                                                       a.ForProperty(auth => auth.Ok).UseAlias("ok");
                                                       a.ForProperty(auth => auth.PreviousLevel).UseAlias("was");
                                                       a.ForProperty(auth => auth.Profile).UseAlias("profile");
                                                   })
                );
        }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        /// <value>The profile.</value>
        public int? Profile { get; set; }
        /// <summary>
        /// Gets or sets the previous profile level.
        /// </summary>
        /// <value>The previous level.</value>
        public double? PreviousLevel { get; set; }
    }
}