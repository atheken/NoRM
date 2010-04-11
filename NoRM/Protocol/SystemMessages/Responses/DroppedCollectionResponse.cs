
namespace Norm.Responses
{
    /// <summary>
    /// The dropped collection response.
    /// </summary>
    public class DroppedCollectionResponse : BaseStatusMessage
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string drop { get; private set; }
        
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? NIndexesWas { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Msg { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Ns { get; private set; }
    }
}