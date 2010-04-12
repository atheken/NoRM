
namespace Norm.Responses
{
    /// <summary>
    /// The validate collection response.
    /// </summary>
    public class ValidateCollectionResponse : BaseStatusMessage
    {
        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Ns { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public string Result { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public bool? Valid { get; set; }

        /// <summary>TODO::Description.</summary>
        /// <value></value>
        public double? LastExtentSize { get; set; }
    }
}