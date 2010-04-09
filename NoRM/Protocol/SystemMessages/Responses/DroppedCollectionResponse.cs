
namespace Norm.Responses
{
    /// <summary>
    /// The dropped collection response.
    /// </summary>
    public class DroppedCollectionResponse
    {
        /// <summary>TODO::Description.</summary>
        public string drop { get; set; }
        
        /// <summary>TODO::Description.</summary>
        public double? NIndexesWas { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Msg { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Ns { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? OK { get; set; }
    }
}