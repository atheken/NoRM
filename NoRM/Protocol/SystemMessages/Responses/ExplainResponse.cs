using Norm.BSON;
using Norm.Configuration;

namespace Norm.Responses
{
    /// <summary>
    /// The explain response.
    /// </summary>
    public class ExplainResponse : ExplainPlan, IFlyweight
    {
        /// <summary>
        /// Explains the plan.
        /// </summary>
        static ExplainResponse()
        {
            MongoConfiguration.Initialize(c => c.For<ExplainResponse>(a =>
            {
                a.ForProperty(auth => auth.NumberScanned).UseAlias("nscanned");
                a.ForProperty(auth => auth.NumberOfScannedObjects).UseAlias("nscannedObjects");
                a.ForProperty(auth => auth.Number).UseAlias("n");
                a.ForProperty(auth => auth.Milliseconds).UseAlias("millis");
                a.ForProperty(auth => auth.OldPlan).UseAlias("oldPlan");
                a.ForProperty(auth => auth.AllPlans).UseAlias("allPlans");
            })
                );
        }

        /// <summary>
        /// Gets the number of objects that would be scanned by this query.
        /// </summary>
        /// <value>The number of objects that will be scanned.</value>
        public int? NumberOfScannedObjects { get; private set; }

        /// <summary>
        /// Gets or sets the number scanned.
        /// </summary>
        /// <value>The number scanned.</value>
        public int NumberScanned { get; private set; }
        
        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        public int Number { get; private set; }
        
        /// <summary>
        /// Gets or sets the milliseconds.
        /// </summary>
        /// <value>The milliseconds.</value>
        public int Milliseconds { get; private set; }
        
        /// <summary>
        /// Gets or sets the old explain plan.
        /// </summary>
        /// <value>The old plan.</value>
        public ExplainPlan OldPlan { get; private set; }
        
        /// <summary>
        /// Gets or sets all explain plans.
        /// </summary>
        /// <value>All plans.</value>
        public ExplainPlan[] AllPlans { get; private set; }
    }
}
