
namespace Norm.Responses
{
    /// <summary>
    /// The validate collection response.
    /// </summary>
    public class ValidateCollectionResponse : BaseStatusMessage
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Ns { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Result { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public bool? Valid { get; private set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? LastExtentSize { get; private set; }
    }
}