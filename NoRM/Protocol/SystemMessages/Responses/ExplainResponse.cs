using NoRM.BSON;

namespace NoRM.Protocol.SystemMessages.Responses
{
    /// <summary>
    /// The explain response.
    /// </summary>
    public class ExplainResponse : Explain, IFlyweight
    {
        public int nscanned { get; set; }
        public int n { get; set; }
        public int millis { get; set; }
        public Explain oldPlan { get; set; }
        public Explain[] allPlans { get; set; }
    }

    /// <summary>
    /// The explain.
    /// </summary>
    public class Explain
    {
        public string cursor { get; set; }
        public Flyweight startKey { get; set; }
        public Flyweight endKey { get; set; }
    }
}