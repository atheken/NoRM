
namespace Norm.Responses
{
    /// <summary>
    /// The dropped collection response.
    /// </summary>
    public class DroppedCollectionResponse : BaseStatusMessage
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
    }
}