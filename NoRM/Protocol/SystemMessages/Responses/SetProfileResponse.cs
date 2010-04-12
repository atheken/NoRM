
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
                                                       a.ForProperty(auth => auth.PreviousLevel).UseAlias("was");
                                                       a.ForProperty(auth => auth.Profile).UseAlias("profile");
                                                   })
                );
        }

        /// <summary>
        /// The profile.
        /// </summary>
        /// <value>The Profile property gets/sets the Profile data member.</value>
        public int? Profile { get; set; }

        /// <summary>
        /// The previous level.
        /// </summary>
        /// <value>The PreviousLevel property gets the PreviousLevel data member.</value>
        public double? PreviousLevel { get; set; }
    }
}