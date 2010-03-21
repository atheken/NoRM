
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
                     a.ForProperty(auth => auth.Ok).UseAlias("ok");
                     a.ForProperty(auth => auth.Version).UseAlias("version");
                     a.ForProperty(auth => auth.GitVersion).UseAlias("gitVersion");
                     a.ForProperty(auth => auth.SystemInformation).UseAlias("sysInfo");
                 }));
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets the git version.
        /// </summary>
        /// <value>The git version.</value>
        public string GitVersion { get; set; }
        /// <summary>
        /// Gets or sets the sys info.
        /// </summary>
        /// <value>The sys info.</value>
        public string SystemInformation { get; set; }
        public int? Bits { get; set; }
    }
}