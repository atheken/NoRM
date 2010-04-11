
namespace Norm.Responses
{
    /// <summary>
    /// The dropped collection response.
    /// </summary>
    public class DroppedCollectionResponse
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string drop { get; set; }
        
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? NIndexesWas { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Msg { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Ns { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? OK { get; set; }
    }
}