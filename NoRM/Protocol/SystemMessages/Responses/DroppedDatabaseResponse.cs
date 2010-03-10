using NoRM.BSON;

namespace NoRM.Responses
{
    /// <summary>
    /// The dropped database response.
    /// </summary>
    public class DroppedDatabaseResponse : IFlyweight
    {
        public string Dropped { get; set; }
        public double? OK { get; set; }
    }
}