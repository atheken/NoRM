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
        /// Indicates if the database asserted.
        /// </summary>
        /// <value>The DatabaseAsserted property gets the DatabaseAsserted data member.</value>
        public bool? DatabaseAsserted { get; set; }

        /// <summary>
        /// Indicates that the database asserted or a user assert has happened
        /// </summary>
        /// <value>The Asserted property gets the Asserted data member.</value>
        public bool? Asserted { get; set; }

        /// <summary>
        /// The assert.
        /// </summary>
        /// <value>The Assert property gets the Assert data member.</value>
        public string Assert { get; set; }

        /// <summary>
        /// The warning assert.
        /// </summary>
        /// <value>The WarningAssert property gets the WarningAssert data member.</value>
        public string WarningAssert { get; set; }

        /// <summary>
        /// The assert message.
        /// </summary>
        /// <value>The AssertMessage property gets the AssertMessage data member.</value>
        public string AssertMessage { get; set; }

        /// <summary>
        /// Gets or sets assert user.
        /// </summary>
        /// <value>The AssertUser property gets the AssertUser data member.</value>
        public string AssertUser { get; set; }
    }
}