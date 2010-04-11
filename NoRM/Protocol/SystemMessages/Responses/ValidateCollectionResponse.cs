
namespace Norm.Responses
{
    /// <summary>
    /// The validate collection response.
    /// </summary>
    public class ValidateCollectionResponse : BaseStatusMessage
    {
        /// <summary>TODO::Description.</summary>
        public string Ns { get; set; }

        /// <summary>TODO::Description.</summary>
        public string Result { get; set; }

        /// <summary>TODO::Description.</summary>
        public bool? Valid { get; set; }

        /// <summary>TODO::Description.</summary>
        public double? LastExtentSize { get; set; }
    }
}