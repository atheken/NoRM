
namespace NoRM.Responses
{
    /// <summary>
    /// The dropped collection response.
    /// </summary>
    public class DroppedCollectionResponse
    {
        public string drop { get; set; }
        public double? NIndexesWas { get; set; }
        public string Msg { get; set; }
        public string Ns { get; set; }
        public double? OK { get; set; }
    }
}