
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The build info response.
    /// </summary>
    public class BuildInfoResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="BuildInfoResponse"/> class.
        /// </summary>
        static BuildInfoResponse()
        {
            MongoConfiguration.Initialize(c => c.For<BuildInfoResponse>(a =>
                 {                     
                     a.ForProperty(auth => auth.Version).UseAlias("version");
                     a.ForProperty(auth => auth.GitVersion).UseAlias("gitVersion");
                     a.ForProperty(auth => auth.SystemInformation).UseAlias("sysInfo");
                 }));
        }

        /// <summary>
        /// The version.
        /// </summary>
        /// <value>The Version property gets the Version data member.</value>
        public string Version { get; set; }

        /// <summary>
        /// The git version.
        /// </summary>
        /// <value>The GitVersion property gets the GitVersion data member.</value>
        public string GitVersion { get; set; }

        /// <summary>
        /// The sys info.
        /// </summary>
        /// <value>The SystemInformation property gets the SystemInformation data member.</value>
        public string SystemInformation { get; set; }

        /// <summary>The number of bits for the current build (32 or 64).</summary>
        /// <value>The Bits property gets the Bits data member.</value>
        public int? Bits { get; set; }
    }
}