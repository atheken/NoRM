using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The assert info response.
    /// </summary>
    public class AssertInfoResponse : BaseStatusMessage
    {
        /// <summary>
        /// Initializes the <see cref="AssertInfoResponse"/> class.
        /// </summary>
        static AssertInfoResponse()
        {
            MongoConfiguration.Initialize(c => c.For<AssertInfoResponse>(a =>
                                                   {
                                                       a.ForProperty(auth => auth.Ok).UseAlias("ok");
                                                       a.ForProperty(auth => auth.DatabaseAsserted).UseAlias("dbasserted");
                                                       a.ForProperty(auth => auth.Asserted).UseAlias("asserted");
                                                       a.ForProperty(auth => auth.Assert).UseAlias("assert");
                                                       a.ForProperty(auth => auth.WarningAssert).UseAlias("assertw");
                                                       a.ForProperty(auth => auth.AssertMessage).UseAlias("assertmsg");
                                                       a.ForProperty(auth => auth.AssertUser).UseAlias("assertuser");
                                                   })
                );
        }

        /// <summary>
        /// Gets or sets the if the database asserted.
        /// </summary>
        /// <value>Database asserted.</value>
        public bool? DatabaseAsserted { get; set; }
        /// <summary>
        /// Gets or sets database asserted or a user assert has happened
        /// </summary>
        /// <value>Database asserted or a user assert has happened.</value>
        public bool? Asserted { get; set; }
        /// <summary>
        /// Gets or sets the regular assert.
        /// </summary>
        /// <value>The assert.</value>
        public string Assert { get; set; }
        /// <summary>
        /// Gets or sets the warning assert.
        /// </summary>
        /// <value>The warning assert.</value>
        public string WarningAssert { get; set; }
        /// <summary>
        /// Gets or sets the assert message.
        /// </summary>
        /// <value>The assert message.</value>
        public string AssertMessage { get; set; }
        /// <summary>
        /// Gets or sets assert user.
        /// </summary>
        /// <value></value>
        public string AssertUser { get; set; }
    }
}